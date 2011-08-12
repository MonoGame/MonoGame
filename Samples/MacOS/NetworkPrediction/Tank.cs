#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
#endregion

namespace NetworkPrediction
{
    /// <summary>
    /// Each player controls a tank, which they can drive around the screen.
    /// This class implements the logic for moving and drawing the tank, sending
    /// and receiving network packets, and applying prediction and smoothing to
    /// compensate for network latency.
    /// </summary>
    class Tank
    {
        #region Constants

        // Constants control how fast the tank moves and turns.
        const float TankTurnRate = 0.01f;
        const float TurretTurnRate = 0.03f;
        const float TankSpeed = 0.3f;
        const float TankFriction = 0.9f;

        #endregion

        #region Fields


        // To implement smoothing, we need more than one copy of the tank state.
        // We must record both where it used to be, and where it is now, an also
        // a smoothed value somewhere in between these two states which is where
        // we will draw the tank on the screen. To simplify managing these three
        // different versions of the tank state, we move all the state fields into
        // this internal helper structure.
        struct TankState
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float TankRotation;
            public float TurretRotation;
        }


        // This is the latest master copy of the tank state, used by our local
        // physics computations and prediction. This state will jerk whenever
        // a new network packet is received.
        TankState simulationState;


        // This is a copy of the state from immediately before the last
        // network packet was received.
        TankState previousState;


        // This is the tank state that is drawn onto the screen. It is gradually
        // interpolated from the previousState toward the simultationState, in
        // order to smooth out any sudden jumps caused by discontinuities when
        // a network packet suddenly modifies the simultationState.
        TankState displayState;


        // Used to interpolate displayState from previousState toward simulationState.
        float currentSmoothing;


        // Averaged time difference from the last 100 incoming packets, used to
        // estimate how our local clock compares to the time on the remote machine.
        RollingAverage clockDelta = new RollingAverage(100);

        
        // Input controls can be read from keyboard, gamepad, or the network.
        Vector2 tankInput;
        Vector2 turretInput;


        // Textures used to draw the tank.
        Texture2D tankTexture;
        Texture2D turretTexture;

        Vector2 screenSize;


        #endregion

        #region Properties


        /// <summary>
        /// Gets the current position of the tank.
        /// </summary>
        public Vector2 Position
        {
            get { return displayState.Position; }
        }


        #endregion


        /// <summary>
        /// Constructs a new Tank instance.
        /// </summary>
        public Tank(int gamerIndex, ContentManager content,
                    int screenWidth, int screenHeight)
        {
            // Use the gamer index to compute a starting position, so each player
            // starts in a different place as opposed to all on top of each other.
            float x = screenWidth / 4 + (gamerIndex % 5) * screenWidth / 8;
            float y = screenHeight / 4 + (gamerIndex / 5) * screenHeight / 5;

            simulationState.Position = new Vector2(x, y);
            simulationState.TankRotation = -MathHelper.PiOver2;
            simulationState.TurretRotation = -MathHelper.PiOver2;

            // Initialize all three versions of our state to the same values.
            previousState = simulationState;
            displayState = simulationState;

            // Load textures.
            tankTexture = content.Load<Texture2D>("Tank");
            turretTexture = content.Load<Texture2D>("Turret");

            screenSize = new Vector2(screenWidth, screenHeight);
        }


        /// <summary>
        /// Moves a locally controlled tank in response to the specified inputs.
        /// </summary>
        public void UpdateLocal(Vector2 tankInput, Vector2 turretInput)
        {
            this.tankInput = tankInput;
            this.turretInput = turretInput;

            // Update the master simulation state.
            UpdateState(ref simulationState);

            // Locally controlled tanks have no prediction or smoothing, so we
            // just copy the simulation state directly into the display state.
            displayState = simulationState;
        }


        /// <summary>
        /// Applies prediction and smoothing to a remotely controlled tank.
        /// </summary>
        public void UpdateRemote(int framesBetweenPackets, bool enablePrediction)
        {
            // Update the smoothing amount, which interpolates from the previous
            // state toward the current simultation state. The speed of this decay
            // depends on the number of frames between packets: we want to finish
            // our smoothing interpolation at the same time the next packet is due.
            float smoothingDecay = 1.0f / framesBetweenPackets;

            currentSmoothing -= smoothingDecay;

            if (currentSmoothing < 0)
                currentSmoothing = 0;

            if (enablePrediction)
            {
                // Predict how the remote tank will move by updating
                // our local copy of its simultation state.
                UpdateState(ref simulationState);

                // If both smoothing and prediction are active,
                // also apply prediction to the previous state.
                if (currentSmoothing > 0)
                {
                    UpdateState(ref previousState);
                }
            }

            if (currentSmoothing > 0)
            {
                // Interpolate the display state gradually from the
                // previous state to the current simultation state.
                ApplySmoothing();
            }
            else
            {
                // Copy the simulation state directly into the display state.
                displayState = simulationState;
            }
        }


        /// <summary>
        /// Applies smoothing by interpolating the display state somewhere
        /// in between the previous state and current simulation state.
        /// </summary>
        void ApplySmoothing()
        {
            displayState.Position = Vector2.Lerp(simulationState.Position,
                                                 previousState.Position,
                                                 currentSmoothing);

            displayState.Velocity = Vector2.Lerp(simulationState.Velocity,
                                                 previousState.Velocity,
                                                 currentSmoothing);

            displayState.TankRotation = MathHelper.Lerp(simulationState.TankRotation,
                                                        previousState.TankRotation,
                                                        currentSmoothing);

            displayState.TurretRotation = MathHelper.Lerp(simulationState.TurretRotation,
                                                          previousState.TurretRotation,
                                                          currentSmoothing);
        }


        /// <summary>
        /// Writes our local tank state into a network packet.
        /// </summary>
        public void WriteNetworkPacket(PacketWriter packetWriter, GameTime gameTime)
        {
            // Send our current time.
            packetWriter.Write((float)gameTime.TotalGameTime.TotalSeconds);

            // Send the current state of the tank.
            packetWriter.Write(simulationState.Position);
            packetWriter.Write(simulationState.Velocity);
            packetWriter.Write(simulationState.TankRotation);
            packetWriter.Write(simulationState.TurretRotation);

            // Also send our current inputs. These can be used to more accurately
            // predict how the tank is likely to move in the future.
            packetWriter.Write(tankInput);
            packetWriter.Write(turretInput);
        }


        /// <summary>
        /// Reads the state of a remotely controlled tank from a network packet.
        /// </summary>
        public void ReadNetworkPacket(PacketReader packetReader,
                                      GameTime gameTime, TimeSpan latency,
                                      bool enablePrediction, bool enableSmoothing)
        {
            if (enableSmoothing)
            {
                // Start a new smoothing interpolation from our current
                // state toward this new state we just received.
                previousState = displayState;
                currentSmoothing = 1;
            }
            else
            {
                currentSmoothing = 0;
            }

            // Read what time this packet was sent.
            float packetSendTime = packetReader.ReadSingle();

            // Read simulation state from the network packet.
            simulationState.Position = packetReader.ReadVector2();
            simulationState.Velocity = packetReader.ReadVector2();
            simulationState.TankRotation = packetReader.ReadSingle();
            simulationState.TurretRotation = packetReader.ReadSingle();

            // Read remote inputs from the network packet.
            tankInput = packetReader.ReadVector2();
            turretInput = packetReader.ReadVector2();

            // Optionally apply prediction to compensate for
            // how long it took this packet to reach us.
            if (enablePrediction)
            {
                ApplyPrediction(gameTime, latency, packetSendTime);
            }
        }


        /// <summary>
        /// Incoming network packets tell us where the tank was at the time the packet
        /// was sent. But packets do not arrive instantly! We want to know where the
        /// tank is now, not just where it used to be. This method attempts to guess
        /// the current state by figuring out how long the packet took to arrive, then
        /// running the appropriate number of local updates to catch up to that time.
        /// This allows us to figure out things like "it used to be over there, and it
        /// was moving that way while turning to the left, so assuming it carried on
        /// using those same inputs, it should now be over here".
        /// </summary>
        void ApplyPrediction(GameTime gameTime, TimeSpan latency, float packetSendTime)
        {
            // Work out the difference between our current local time
            // and the remote time at which this packet was sent.
            float localTime = (float)gameTime.TotalGameTime.TotalSeconds;

            float timeDelta = localTime - packetSendTime;

            // Maintain a rolling average of time deltas from the last 100 packets.
            clockDelta.AddValue(timeDelta);

            // The caller passed in an estimate of the average network latency, which
            // is provided by the XNA Framework networking layer. But not all packets
            // will take exactly that average amount of time to arrive! To handle
            // varying latencies per packet, we include the send time as part of our
            // packet data. By comparing this with a rolling average of the last 100
            // send times, we can detect packets that are later or earlier than usual,
            // even without having synchronized clocks between the two machines. We
            // then adjust our average latency estimate by this per-packet deviation.

            float timeDeviation = timeDelta - clockDelta.AverageValue;

            latency += TimeSpan.FromSeconds(timeDeviation);

            TimeSpan oneFrame = TimeSpan.FromSeconds(1.0 / 60.0);

            // Apply prediction by updating our simulation state however
            // many times is necessary to catch up to the current time.
            while (latency >= oneFrame)
            {
                UpdateState(ref simulationState);

                latency -= oneFrame;
            }
        }


        /// <summary>
        /// Updates one of our state structures, using the current inputs to turn
        /// the tank, and applying the velocity and inertia calculations. This
        /// method is used directly to update locally controlled tanks, and also
        /// indirectly to predict the motion of remote tanks.
        /// </summary>
        void UpdateState(ref TankState state)
        {
            // Gradually turn the tank and turret to face the requested direction.
            state.TankRotation = TurnToFace(state.TankRotation,
                                            tankInput, TankTurnRate);
            
            state.TurretRotation = TurnToFace(state.TurretRotation,
                                              turretInput, TurretTurnRate);

            // How close the desired direction is the tank facing?
            Vector2 tankForward = new Vector2((float)Math.Cos(state.TankRotation),
                                              (float)Math.Sin(state.TankRotation));

            Vector2 targetForward = new Vector2(tankInput.X, -tankInput.Y);

            float facingForward = Vector2.Dot(tankForward, targetForward);

            // If we have finished turning, also start moving forward.
            if (facingForward > 0)
            {
                float speed = facingForward * facingForward * TankSpeed;

                state.Velocity += tankForward * speed;
            }

            // Update the position and velocity.
            state.Position += state.Velocity;
            state.Velocity *= TankFriction;

            // Clamp so the tank cannot drive off the edge of the screen.
            state.Position = Vector2.Clamp(state.Position, Vector2.Zero, screenSize);
        }


        /// <summary>
        /// Gradually rotates the tank to face the specified direction.
        /// See the Aiming sample (creators.xna.com) for details.
        /// </summary>
        static float TurnToFace(float rotation, Vector2 target, float turnRate)
        {
            if (target == Vector2.Zero)
                return rotation;

            float angle = (float)Math.Atan2(-target.Y, target.X);

            float difference = rotation - angle;

            while (difference > MathHelper.Pi)
                difference -= MathHelper.TwoPi;

            while (difference < -MathHelper.Pi)
                difference += MathHelper.TwoPi;

            turnRate *= Math.Abs(difference);

            if (difference < 0)
                return rotation + Math.Min(turnRate, -difference);
            else
                return rotation - Math.Min(turnRate, difference);
        }


        /// <summary>
        /// Draws the tank and turret.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(tankTexture.Width / 2, tankTexture.Height / 2);

            spriteBatch.Draw(tankTexture, displayState.Position, null, Color.White,
                             displayState.TankRotation, origin, 1,
                             SpriteEffects.None, 0);

            spriteBatch.Draw(turretTexture, displayState.Position, null, Color.White,
                             displayState.TurretRotation, origin, 1,
                             SpriteEffects.None, 0);
        }
    }
}
