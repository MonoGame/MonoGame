using System;
using System.IO;
using NUnit.Framework;

namespace MonoGame.Tests
{
    partial class PipelineToolTests
    {
        public void FileTests()
        {
            // Test starting state
            Assert.AreEqual(_controller.ProjectOpen, false);
            Assert.AreEqual(_controller.ProjectDirty, false);

            // Create New Project
            var _newPath = Path.Combine(_workingDir, "TestProject0.mgcb");
            _view.AskSaveNameFilePath = _newPath;
            _view.AskSaveNameResult = true;
            _controller.NewProject();
            Assert.AreEqual(_controller.ProjectOpen, true);
            Assert.AreEqual(_controller.ProjectDirty, true);
            Assert.AreEqual(_controller.ProjectLocation, _workingDir + Path.DirectorySeparatorChar);

            // Save Project
            _controller.SaveProject(false);
            Assert.AreEqual(_controller.ProjectDirty, false);

            // Close Project
            _controller.CloseProject();
            Assert.AreEqual(_controller.ProjectOpen, false);

            // Delete created stuff so that the test can be run again
            File.Delete(_newPath);
        }
    }
}

