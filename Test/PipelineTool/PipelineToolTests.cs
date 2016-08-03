using System;
using System.IO;
using MonoGame.Tools.Pipeline;
using NUnit.Framework;

namespace MonoGame.Tests
{
    partial class PipelineToolTests
    {
        private string _workingDir;
        private FakeView _view;
        private PipelineController _controller;

        [Test]
        public void TestTool()
        {
            _workingDir = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Assets"), "PipelineProjects");
            _view = new FakeView();
            _controller = PipelineController.Create(_view);

            FileTests();
        }
    }
}

