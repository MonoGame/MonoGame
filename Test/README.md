# MonoGame Tests

The MonoGame Tests run against XNA on Windows and MonoGame on Windows,
Mac OS X and Linux.  They serve as an assurance that MonoGame conforms
as closely as possible to XNA.

Simple unit tests make assertions about MonoGame's core class
properties, methods and behavior to guarantee compatibility with XNA in
those regards.  Additionally, visual tests verify via frame capture and
comparison that MonoGame renders equivalently to XNA.

Currently, on Windows, the tests can be run using NUnit and target
either XNA or MonoGame.  On Mac OS X and Linux, the tests target
MonoGame and are implemented in an executable assembly that can be run
and debugged directly.  After execution using the custom test runner,
and HTML report of the results will be loaded in your default browser,
and a log of stdout can be found in bin\$(Configuration)\stdout.txt.

*Note: Currently there is no way to skip or select certain tests to run
using the custom runner.  This functionality is coming soon.*

## Visual Tests

MonoGame's visual tests are implemented as ```Game```s and
```GameComponent```s whose output is captured and compared to known-good
reference renderings.  Performance is ignored in these tests:  the focus
here is correct drawing.

New one-off visual tests may be added to ```BasicVisualTest```,
```IntermediateVisualTest``` and ```AdvancedVisualTest```.  Alternately,
new sets of related visual tests can be conveniently grouped and
implemented in a new class derived from ```VisualTestFixtureBase```.

### Workflow for Implementing a Visual Test

A good visual test, like any good test, should perform the minimum work
necessary to verify that the functionality under test is correct.  As
much as possible, drawing and test code should be made modular by
inheriting from ```GameComponent```, ```DrawableGameComponent```,
```VisualTestGameComponent```, or ```VisualTestDrawableGameComponent```
to encourage reuse, rather than duplication, in other tests.

Here is one possible workflow for implementing a visual test (assuming
the test fixture is already set up):

> Before you start:  
> Examine the existing visual tests to get a sense of how components
> are being composed into a test and what components may already
> satisify part of the requirements of the new test.


1. Implement any new drawing logic needed in a new subclass of one of
   the \*Component base classes.

2. Compose your test Game in a new [Test] method.  As this stage, you
   can run the new test directly to visually verify the rendering.

3. Add a FrameCompareComponent to your test Game with at least one
   IFrameComparer implementation.  Use the ```FrameCompareComponent```'s
   predicate to capture and compare frames.

4. Pass the FrameCompareComponent.Results to the diffing, logging and
   assertion utility methods provided by ```VisualTestFixtureBase```

5. The first time a visual test is run, it will fail for lack of
   reference images to compare the captured images to.  However, it will
   write the captured frames to bin\$(Configuration)\CapturedFrames\{TestDir}.

6. Proof the images generated from the first run to ensure that they are
   correct, then add them to the test project in
   Assets\ReferenceImages\$TestDir.  **Be sure to add them in the
   projects for all platforms!**  These files should have their build
   actions set to "Compile" and "Copy if newer".

7. Rerun the visual test.  The reference images should be copied into
   place and the test should now pass.

8. XOR diffs between the reference images and captured frames are output
   into bin\$(Configuration)\Diffs\{TestDir} for debugging purposes.


### Notes For Implementing Correct Visual Tests

- **Visual tests must be marked as [RequiresSTA] to work correctly on
  all platforms**
- Try not to rely on ```GameComponent.Update```, since capturing frames
  can make the game run slowly and result in multiple Update calls per
  Draw.  Instead, inherit from ```VisualTestGameComponent``` or
  ```VisualTestDrawableGameComponent``` and override
  ```UpdateOncePerDraw```.
- For similarity thresholds, prefer Constants.StandardRequiredSimilarity
  unless there is a very good reason to choose another value.  This will
  allow the strictness of all the tests to be adjusted together in the
  future, as requirements change.
- Use the Paths static class to reduce typos in resource paths.

## Special Considerations

- For new test fixtures, call Paths.SetStandardWorkingDirectory() in
  [SetUp] \(VisualTestBase does this for you\) to ensure that the
  ```ContentManager``` can find your assets on all platforms.
- Note that all platforms are forced to run in Synchronous mode and
  that this doesn't always work perfectly on all platforms yet.

## NUnit Configuration

There are a few things to know about running these tests under NUnit:

- You must run the -x86 versions of NUnit, because XNA won't work with
  the 64-bit versions.
- You must disable shadow copying because having it enabled makes it
  impossible for the ContentManager to find any assets.
  - GUI: Tools > Settings > Test Loader > Advanced
  - CLI: /noshadow
- For debugger support, Run tests directly in the NUnit process, (note
  that this may cause a few-seconds-long hang when exiting NUnit after
  running a visual test) otherwise choose 'single separate process'
  - This setting can be found in:
    ```Tools > Settings > Test Loader > Assembly Isolation```
