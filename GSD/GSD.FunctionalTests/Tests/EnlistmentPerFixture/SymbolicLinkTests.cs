﻿using System.IO;
using GSD.FunctionalTests.FileSystemRunners;
using GSD.FunctionalTests.Should;
using GSD.FunctionalTests.Tools;
using GSD.Tests.Should;
using NUnit.Framework;

namespace GSD.FunctionalTests.Tests.EnlistmentPerFixture
{
    // MacOnly until issue #297 (add SymLink support for Windows) is complete
    [Category(Categories.MacOnly)]
    [TestFixture]
    public class SymbolicLinkTests : TestsWithEnlistmentPerFixture
    {
        private const string TestFolderName = "Test_EPF_SymbolicLinks";

        // FunctionalTests/20180925_SymLinksPart1 files
        private const string TestFileName = "TestFile.txt";
        private const string TestFileContents = "This is a real file";
        private const string TestFile2Name = "TestFile2.txt";
        private const string TestFile2Contents = "This is the second real file";
        private const string ChildFolderName = "ChildDir";
        private const string ChildLinkName = "LinkToFileInFolder";
        private const string GrandChildLinkName = "LinkToFileInParentFolder";

        // FunctionalTests/20180925_SymLinksPart2 files
        // Note: In this branch ChildLinkName has been changed to point to TestFile2Name
        private const string GrandChildFileName = "TestFile3.txt";
        private const string GrandChildFileContents = "This is the third file";
        private const string GrandChildLinkNowAFileContents = "This was a link but is now a file";

        // FunctionalTests/20180925_SymLinksPart3 files
        private const string ChildFolder2Name = "ChildDir2";

        // FunctionalTests/20180925_SymLinksPart4 files
        // Note: In this branch ChildLinkName has been changed to a directory and ChildFolder2Name has been changed to a link to ChildFolderName

        private BashRunner bashRunner;
        public SymbolicLinkTests()
        {
            this.bashRunner = new BashRunner();
        }

        [TestCase, Order(1)]
        public void CheckoutBranchWithSymLinks()
        {
            GitHelpers.InvokeGitAgainstGSDRepo(this.Enlistment.RepoRoot, "checkout FunctionalTests/20180925_SymLinksPart1");
            GitHelpers.CheckGitCommandAgainstGSDRepo(
                this.Enlistment.RepoRoot,
                "status",
                "On branch FunctionalTests/20180925_SymLinksPart1",
                "nothing to commit, working tree clean");

            string testFilePath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, TestFileName));
            testFilePath.ShouldBeAFile(this.bashRunner).WithContents(TestFileContents);
            this.bashRunner.IsSymbolicLink(testFilePath).ShouldBeFalse($"{testFilePath} should not be a symlink");

            string testFile2Path = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, TestFile2Name));
            testFile2Path.ShouldBeAFile(this.bashRunner).WithContents(TestFile2Contents);
            this.bashRunner.IsSymbolicLink(testFile2Path).ShouldBeFalse($"{testFile2Path} should not be a symlink");

            string childLinkPath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildLinkName));
            this.bashRunner.IsSymbolicLink(childLinkPath).ShouldBeTrue($"{childLinkPath} should be a symlink");
            childLinkPath.ShouldBeAFile(this.bashRunner).WithContents(TestFileContents);

            string grandChildLinkPath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildFolderName, GrandChildLinkName));
            this.bashRunner.IsSymbolicLink(grandChildLinkPath).ShouldBeTrue($"{grandChildLinkPath} should be a symlink");
            grandChildLinkPath.ShouldBeAFile(this.bashRunner).WithContents(TestFile2Contents);
        }

        [TestCase, Order(2)]
        public void CheckoutBranchWhereSymLinksChangeContentsAndTransitionToFile()
        {
            GitHelpers.InvokeGitAgainstGSDRepo(this.Enlistment.RepoRoot, "checkout FunctionalTests/20180925_SymLinksPart2");
            GitHelpers.CheckGitCommandAgainstGSDRepo(
                this.Enlistment.RepoRoot,
                "status",
                "On branch FunctionalTests/20180925_SymLinksPart2",
                "nothing to commit, working tree clean");

            // testFilePath and testFile2Path are unchanged from FunctionalTests/20180925_SymLinksPart2
            string testFilePath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, TestFileName));
            testFilePath.ShouldBeAFile(this.bashRunner).WithContents(TestFileContents);
            this.bashRunner.IsSymbolicLink(testFilePath).ShouldBeFalse($"{testFilePath} should not be a symlink");

            string testFile2Path = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, TestFile2Name));
            testFile2Path.ShouldBeAFile(this.bashRunner).WithContents(TestFile2Contents);
            this.bashRunner.IsSymbolicLink(testFile2Path).ShouldBeFalse($"{testFile2Path} should not be a symlink");

            // In this branch childLinkPath has been changed to point to testFile2Path
            string childLinkPath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildLinkName));
            this.bashRunner.IsSymbolicLink(childLinkPath).ShouldBeTrue($"{childLinkPath} should be a symlink");
            childLinkPath.ShouldBeAFile(this.bashRunner).WithContents(TestFile2Contents);

            // grandChildLinkPath should now be a file
            string grandChildLinkPath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildFolderName, GrandChildLinkName));
            this.bashRunner.IsSymbolicLink(grandChildLinkPath).ShouldBeFalse($"{grandChildLinkPath} should not be a symlink");
            grandChildLinkPath.ShouldBeAFile(this.bashRunner).WithContents(GrandChildLinkNowAFileContents);

            // There should also be a new file in the child folder
            string newGrandChildFilePath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildFolderName, GrandChildFileName));
            newGrandChildFilePath.ShouldBeAFile(this.bashRunner).WithContents(GrandChildFileContents);
            this.bashRunner.IsSymbolicLink(newGrandChildFilePath).ShouldBeFalse($"{newGrandChildFilePath} should not be a symlink");
        }

        [TestCase, Order(3)]
        public void CheckoutBranchWhereFilesTransitionToSymLinks()
        {
            GitHelpers.InvokeGitAgainstGSDRepo(this.Enlistment.RepoRoot, "checkout FunctionalTests/20180925_SymLinksPart3");
            GitHelpers.CheckGitCommandAgainstGSDRepo(
                this.Enlistment.RepoRoot,
                "status",
                "On branch FunctionalTests/20180925_SymLinksPart3",
                "nothing to commit, working tree clean");

            // In this branch testFilePath has been changed to point to newGrandChildFilePath
            string testFilePath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, TestFileName));
            testFilePath.ShouldBeAFile(this.bashRunner).WithContents(GrandChildFileContents);
            this.bashRunner.IsSymbolicLink(testFilePath).ShouldBeTrue($"{testFilePath} should be a symlink");

            // There should be a new ChildFolder2Name directory
            string childFolder2Path = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildFolder2Name));
            this.bashRunner.IsSymbolicLink(childFolder2Path).ShouldBeFalse($"{childFolder2Path} should not be a symlink");
            childFolder2Path.ShouldBeADirectory(this.bashRunner);

            // The rest of the files are unchanged from FunctionalTests/20180925_SymLinksPart2
            string testFile2Path = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, TestFile2Name));
            testFile2Path.ShouldBeAFile(this.bashRunner).WithContents(TestFile2Contents);
            this.bashRunner.IsSymbolicLink(testFile2Path).ShouldBeFalse($"{testFile2Path} should not be a symlink");

            string childLinkPath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildLinkName));
            this.bashRunner.IsSymbolicLink(childLinkPath).ShouldBeTrue($"{childLinkPath} should be a symlink");
            childLinkPath.ShouldBeAFile(this.bashRunner).WithContents(TestFile2Contents);

            string grandChildLinkPath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildFolderName, GrandChildLinkName));
            this.bashRunner.IsSymbolicLink(grandChildLinkPath).ShouldBeFalse($"{grandChildLinkPath} should not be a symlink");
            grandChildLinkPath.ShouldBeAFile(this.bashRunner).WithContents(GrandChildLinkNowAFileContents);

            string newGrandChildFilePath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildFolderName, GrandChildFileName));
            newGrandChildFilePath.ShouldBeAFile(this.bashRunner).WithContents(GrandChildFileContents);
            this.bashRunner.IsSymbolicLink(newGrandChildFilePath).ShouldBeFalse($"{newGrandChildFilePath} should not be a symlink");
        }

        [TestCase, Order(4)]
        public void CheckoutBranchWhereSymLinkTransistionsToFolderAndFolderTransitionsToSymlink()
        {
            GitHelpers.InvokeGitAgainstGSDRepo(this.Enlistment.RepoRoot, "checkout FunctionalTests/20180925_SymLinksPart4");
            GitHelpers.CheckGitCommandAgainstGSDRepo(
                this.Enlistment.RepoRoot,
                "status",
                "On branch FunctionalTests/20180925_SymLinksPart4",
                "nothing to commit, working tree clean");

            // In this branch ChildLinkName has been changed to a directory and ChildFolder2Name has been changed to a link to ChildFolderName
            string linkNowADirectoryPath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildLinkName));
            this.bashRunner.IsSymbolicLink(linkNowADirectoryPath).ShouldBeFalse($"{linkNowADirectoryPath} should not be a symlink");
            linkNowADirectoryPath.ShouldBeADirectory(this.bashRunner);

            string directoryNowALinkPath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, ChildFolder2Name));
            this.bashRunner.IsSymbolicLink(directoryNowALinkPath).ShouldBeTrue($"{directoryNowALinkPath} should be a symlink");
        }

        [TestCase, Order(5)]
        public void GitStatusReportsSymLinkChanges()
        {
            GitHelpers.CheckGitCommandAgainstGSDRepo(
                this.Enlistment.RepoRoot,
                "status",
                "On branch FunctionalTests/20180925_SymLinksPart4",
                "nothing to commit, working tree clean");

            string testFilePath = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, TestFileName));
            testFilePath.ShouldBeAFile(this.bashRunner).WithContents(GrandChildFileContents);
            this.bashRunner.IsSymbolicLink(testFilePath).ShouldBeTrue($"{testFilePath} should be a symlink");

            string testFile2Path = this.Enlistment.GetVirtualPathTo(Path.Combine(TestFolderName, TestFile2Name));
            testFile2Path.ShouldBeAFile(this.bashRunner).WithContents(TestFile2Contents);
            this.bashRunner.IsSymbolicLink(testFile2Path).ShouldBeFalse($"{testFile2Path} should not be a symlink");

            // Update testFilePath's symlink to point to testFile2Path
            this.bashRunner.CreateSymbolicLink(testFilePath, testFile2Path);

            testFilePath.ShouldBeAFile(this.bashRunner).WithContents(TestFile2Contents);
            this.bashRunner.IsSymbolicLink(testFilePath).ShouldBeTrue($"{testFilePath} should be a symlink");

            GitHelpers.CheckGitCommandAgainstGSDRepo(
                this.Enlistment.RepoRoot,
                "status",
                "On branch FunctionalTests/20180925_SymLinksPart4",
                $"modified:   {TestFolderName}/{TestFileName}");
        }
    }
}
