// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Manages the playback of a sound or set of sounds.</summary>
    /// <remarks>
    /// <para>Cues are comprised of one or more sounds.</para>
    /// <para>Cues also define specific properties such as pitch or volume.</para>
    /// <para>Cues are referenced through SoundBank objects.</para>
    /// </remarks>
    public class Cue : IDisposable
    {
        private readonly AudioEngine _engine;
        private readonly string _name;
        private readonly XactSound[] _sounds;
        private readonly float[] _probs;

        private readonly RpcVariable[] _variables;

        private XactSound _curSound;

        private bool _applied3D;
        private bool _played;

        /// <summary>Indicates whether or not the cue is currently paused.</summary>
        /// <remarks>IsPlaying and IsPaused both return true if a cue is paused while playing.</remarks>
        public bool IsPaused
        {
            get 
            {
                if (_curSound != null)
                    return _curSound.IsPaused;

                return false;
            }
        }

        /// <summary>Indicates whether or not the cue is currently playing.</summary>
        /// <remarks>IsPlaying and IsPaused both return true if a cue is paused while playing.</remarks>
        public bool IsPlaying
        {
            get 
            {
                if (_curSound != null)
                    return _curSound.Playing || _curSound.IsPaused;

                return false;
            }
        }

        /// <summary>Indicates whether or not the cue is currently stopped.</summary>
        public bool IsStopped
        {
            get 
            {
                if (_curSound != null)
                    return _curSound.Stopped;

                return !IsDisposed && !IsPrepared;
            }
        }

        /// <summary>Returns whether the cue is stopping playback.</summary>
        /// <remarks>Current implementation will always return <see langword="false"/>.</remarks>
        public bool IsStopping
        {
            get
            {
                // TODO: Implement me!
                return false;
            }
        }

        /// <summary>Returns whether the cue is preparing to play.</summary>
        /// <remarks>Current implementation will always return <see langword="false"/>.</remarks>
        public bool IsPreparing 
        {
            get { return false; }
        }

        /// <summary>Returns whether the cue is prepared to play.</summary>
        /// <remarks>
        /// This property returns <see langword="true"/> only if the cue is prepared but has not yet been played.
        /// Cues that are played return <see langword="true"/>
        /// for <see cref="IsPlaying"/> if they are currently playing
        /// or <see langword="true"/> for <see cref="IsStopped"/> if they are stopped.
        /// </remarks>
        public bool IsPrepared { get; internal set; }

        /// <summary>Returns whether the cue has been created.</summary>
        /// <remarks>Current implementation will always return <see langword="false"/>.</remarks>
        public bool IsCreated { get; internal set; }

        /// <summary>Gets the friendly name of the cue.</summary>
        /// <remarks>The friendly name is a value set from the designer.</remarks>
        public string Name
        {
            get { return _name; }
        }
        
        internal Cue(AudioEngine engine, string cuename, XactSound sound)
        {
            _engine = engine;
            _name = cuename;
            _sounds = new XactSound[1];
            _sounds[0] = sound;
            _probs = new float[1];
            _probs[0] = 1.0f;
            _variables = engine.CreateCueVariables();
        }
        
        internal Cue(AudioEngine engine, string cuename, XactSound[] sounds, float[] probs)
        {
            _engine = engine;
            _name = cuename;
            _sounds = sounds;
            _probs = probs;
            _variables = engine.CreateCueVariables();
        }

        internal void Prepare()
        {
            IsDisposed = false;
            IsCreated = false;
            IsPrepared = true;
            _curSound = null;
        }

        /// <summary>Pauses playback.</summary>
        public void Pause()
        {
            lock (_engine.UpdateLock)
            {
                if (_curSound != null)
                    _curSound.Pause();
            }
        }

        /// <summary>Requests playback of a prepared or preparing Cue.</summary>
        /// <remarks>Calling Play when the Cue already is playing can result in an InvalidOperationException.</remarks>
        public void Play()
        {
            lock (_engine.UpdateLock)
            {
                if (!_engine.ActiveCues.Contains(this))
                    _engine.ActiveCues.Add(this);

                //TODO: Probabilities
                var index = XactHelpers.Random.Next(_sounds.Length);
                _curSound = _sounds[index];

                var volume = UpdateRpcCurves();

                _curSound.Play(volume, _engine);
            }

            _played = true;
            IsPrepared = false;
        }

        /// <summary>Resumes playback of a paused Cue.</summary>
        public void Resume()
        {
            lock (_engine.UpdateLock)
            {
                if (_curSound != null)
                    _curSound.Resume();
            }
        }

        /// <summary>Stops playback of a Cue.</summary>
        /// <param name="options">Specifies if the sound should play any pending release phases or transitions before stopping.</param>
        public void Stop(AudioStopOptions options)
        {
            lock (_engine.UpdateLock)
            {
                _engine.ActiveCues.Remove(this);

                if (_curSound != null)
                    _curSound.Stop(options);
            }

            IsPrepared = false;
        }

        private int FindVariable(string name)
        {
            // Do a simple linear search... which is fast
            // for as little variables as most cues have.
            for (var i = 0; i < _variables.Length; i++)
            {
                if (_variables[i].Name == name)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Sets the value of a cue-instance variable based on its friendly name.
        /// </summary>
        /// <param name="name">Friendly name of the variable to set.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <remarks>The friendly name is a value set from the designer.</remarks>
        public void SetVariable(string name, float value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var i = FindVariable(name);
            if (i == -1 || !_variables[i].IsPublic)
                throw new IndexOutOfRangeException("The specified variable index is invalid.");

            _variables[i].SetValue(value);
        }

        /// <summary>Gets a cue-instance variable value based on its friendly name.</summary>
        /// <param name="name">Friendly name of the variable.</param>
        /// <returns>Value of the variable.</returns>
        /// <remarks>
        /// <para>Cue-instance variables are useful when multiple instantiations of a single cue (and its associated sounds) are required (for example, a "car" cue where there may be more than one car at any given time). While a global variable allows multiple audio elements to be controlled in unison, a cue instance variable grants discrete control of each instance of a cue, even for each copy of the same cue.</para>
        /// <para>The friendly name is a value set from the designer.</para>
        /// </remarks>
        public float GetVariable(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var i = FindVariable(name);
            if (i == -1 || !_variables[i].IsPublic)
                throw new IndexOutOfRangeException("The specified variable index is invalid.");

            return _variables[i].Value;
        }

        /// <summary>Updates the simulated 3D Audio settings calculated between an AudioEmitter and AudioListener.</summary>
        /// <param name="listener">The listener to calculate.</param>
        /// <param name="emitter">The emitter to calculate.</param>
        /// <remarks>
        /// <para>This must be called before Play().</para>
        /// <para>Calling this method automatically converts the sound to monoaural and sets the speaker mix for any sound played by this cue to a value calculated with the listener's and emitter's positions. Any stereo information in the sound will be discarded.</para>
        /// </remarks>
        public void Apply3D(AudioListener listener, AudioEmitter emitter) 
        {
            if (listener == null)
                throw new ArgumentNullException("listener");
            if (emitter == null)
                throw new ArgumentNullException("emitter");

            if (_played && !_applied3D)
                throw new InvalidOperationException("You must call Apply3D on a Cue before calling Play to be able to call Apply3D after calling Play.");

            var direction = listener.Position - emitter.Position;

            lock (_engine.UpdateLock)
            {
                // Set the distance for falloff.
                var distance = direction.Length();
                var i = FindVariable("Distance");
                _variables[i].SetValue(distance);

                // Calculate the orientation.
                if (distance > 0.0f)
                    direction /= distance;
                var right = Vector3.Cross(listener.Up, listener.Forward);
                var slope = Vector3.Dot(direction, listener.Forward);
                var angle = MathHelper.ToDegrees(MathF.Acos(slope));
                var j = FindVariable("OrientationAngle");
                _variables[j].SetValue(angle);
                if (_curSound != null)
                    _curSound.SetCuePan(Vector3.Dot(direction, right));

                // Calculate doppler effect.
                var relativeVelocity = emitter.Velocity - listener.Velocity;
                relativeVelocity *= emitter.DopplerScale;
            }

            _applied3D = true;
        }

        internal void Update(float dt)
        {
            if (_curSound == null)
                return;

            _curSound.Update(dt);

            UpdateRpcCurves();
        }

        private float UpdateRpcCurves()
        {
            var volume = 1.0f;

            // Evaluate the runtime parameter controls.
            var rpcCurves = _curSound.RpcCurves;
            if (rpcCurves.Length > 0)
            {
                var pitch = 0.0f;
                var reverbMix = 1.0f;
                float? filterFrequency = null;
                float? filterQFactor = null;

                for (var i = 0; i < rpcCurves.Length; i++)
                {
                    var rpcCurve = _engine.RpcCurves[rpcCurves[i]];

                    // Some curves are driven by global variables and others by cue instance variables.
                    float value;
                    if (rpcCurve.IsGlobal)
                        value = rpcCurve.Evaluate(_engine.GetGlobalVariable(rpcCurve.Variable));
                    else
                        value = rpcCurve.Evaluate(_variables[rpcCurve.Variable].Value);

                    // Process the final curve value based on the parameter type it is.
                    switch (rpcCurve.Parameter)
                    {
                        case RpcParameter.Volume:
                            volume *= XactHelpers.ParseVolumeFromDecibels(value / 100.0f);
                            break;

                        case RpcParameter.Pitch:
                            pitch += value / 1000.0f;
                            break;

                        case RpcParameter.ReverbSend:
                            reverbMix *= XactHelpers.ParseVolumeFromDecibels(value / 100.0f);
                            break;

                        case RpcParameter.FilterFrequency:
                            filterFrequency = value;
                            break;

                        case RpcParameter.FilterQFactor:
                            filterQFactor = value;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException("rpcCurve.Parameter");
                    }
                }

                pitch = MathHelper.Clamp(pitch, -1.0f, 1.0f);
                if (volume < 0.0f)
                    volume = 0.0f;

                _curSound.UpdateState(_engine, volume, pitch, reverbMix, filterFrequency, filterQFactor);
            }

            return volume;
        }
        
        /// <summary>
        /// This event is triggered when the Cue is disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Is true if the Cue has been disposed.
        /// </summary>
        public bool IsDisposed { get; internal set; }

        /// <summary>
        /// Disposes the Cue.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (disposing)
            {
                IsCreated = false;
                IsPrepared = false;
                EventHelpers.Raise(this, Disposing, EventArgs.Empty);
            }
        }
    }
}

