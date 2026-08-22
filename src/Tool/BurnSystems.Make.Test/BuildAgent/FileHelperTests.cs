using BurnSystems.Make.BuildAgent;
using NUnit.Framework;

namespace BSMake.Tests.BuildAgent;

/// <summary>
/// Tests the merging of files, especially the check whether the target file is outdated
/// </summary>
[TestFixture]
public class FileHelperTests
{
    /// <summary>
    /// Stores the temporary directory in which the files of one single test are created
    /// </summary>
    private string _directory = string.Empty;

    /// <summary>
    /// Stores a point in time being clearly in the past, so the timestamps of the test files
    /// can be set explicitly instead of relying upon the resolution of the filesystem
    /// </summary>
    private static readonly DateTime Past = new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private string FirstFile => Path.Combine(_directory, "first.txt");

    private string SecondFile => Path.Combine(_directory, "second.txt");

    private string TargetFile => Path.Combine(_directory, "target.txt");

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "BSMake.FileHelperTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    [Test]
    public void TestMergingOfFiles()
    {
        CreateSourceFiles();

        FileHelper.MergeFiles(CreateOptions(onlyIfModified: false));

        Assert.That(File.ReadAllText(TargetFile), Is.EqualTo("PREFIX\nFIRST\nSECOND\n"));
    }

    [Test]
    public void TestMergingWithoutPrefix()
    {
        CreateSourceFiles();

        var options = CreateOptions(onlyIfModified: false);
        options.PrefixTextTarget = string.Empty;
        FileHelper.MergeFiles(options);

        Assert.That(File.ReadAllText(TargetFile), Is.EqualTo("FIRST\nSECOND\n"));
    }

    /// <summary>
    /// A missing target file is always outdated, so the merge has to be performed
    /// </summary>
    [Test]
    public void TestOutdatedByMissingTargetFile()
    {
        CreateSourceFiles();

        FileHelper.MergeFiles(CreateOptions(onlyIfModified: true));

        Assert.That(File.Exists(TargetFile), Is.True);
        Assert.That(File.ReadAllText(TargetFile), Is.EqualTo("PREFIX\nFIRST\nSECOND\n"));
    }

    /// <summary>
    /// The target file is newer than both source files, so the merge shall be skipped
    /// </summary>
    [Test]
    public void TestNotOutdatedByNewerTargetFile()
    {
        CreateSourceFiles();
        CreateTargetFile(Past.AddHours(1));

        FileHelper.MergeFiles(CreateOptions(onlyIfModified: true));

        Assert.That(File.ReadAllText(TargetFile), Is.EqualTo("OLD TARGET"));
        Assert.That(File.GetLastWriteTimeUtc(TargetFile), Is.EqualTo(Past.AddHours(1)));
    }

    /// <summary>
    /// The first source file is newer than the target file, so the merge has to be performed
    /// </summary>
    [Test]
    public void TestOutdatedByNewerFirstFile()
    {
        CreateSourceFiles();
        CreateTargetFile(Past.AddHours(1));
        File.SetLastWriteTimeUtc(FirstFile, Past.AddHours(2));

        FileHelper.MergeFiles(CreateOptions(onlyIfModified: true));

        Assert.That(File.ReadAllText(TargetFile), Is.EqualTo("PREFIX\nFIRST\nSECOND\n"));
    }

    /// <summary>
    /// The second source file is newer than the target file, so the merge has to be performed
    /// </summary>
    [Test]
    public void TestOutdatedByNewerSecondFile()
    {
        CreateSourceFiles();
        CreateTargetFile(Past.AddHours(1));
        File.SetLastWriteTimeUtc(SecondFile, Past.AddHours(2));

        FileHelper.MergeFiles(CreateOptions(onlyIfModified: true));

        Assert.That(File.ReadAllText(TargetFile), Is.EqualTo("PREFIX\nFIRST\nSECOND\n"));
    }

    /// <summary>
    /// Source files having exactly the same timestamp as the target file do not trigger a merge
    /// </summary>
    [Test]
    public void TestNotOutdatedBySameTimestamp()
    {
        CreateSourceFiles();
        CreateTargetFile(Past);

        FileHelper.MergeFiles(CreateOptions(onlyIfModified: true));

        Assert.That(File.ReadAllText(TargetFile), Is.EqualTo("OLD TARGET"));
    }

    /// <summary>
    /// Without the check for the modification, the merge is performed even for a newer target file
    /// </summary>
    [Test]
    public void TestMergingOfNewerTargetFileWithoutModificationCheck()
    {
        CreateSourceFiles();
        CreateTargetFile(Past.AddHours(1));

        FileHelper.MergeFiles(CreateOptions(onlyIfModified: false));

        Assert.That(File.ReadAllText(TargetFile), Is.EqualTo("PREFIX\nFIRST\nSECOND\n"));
    }

    [Test]
    public void TestMergingOfMissingFileThrows()
    {
        CreateSourceFiles();
        File.Delete(SecondFile);

        Assert.Throws<FileNotFoundException>(() => FileHelper.MergeFiles(CreateOptions(onlyIfModified: true)));
    }

    /// <summary>
    /// The target file must not be one of the source files, otherwise the source would be
    /// truncated before being read
    /// </summary>
    [Test]
    public void TestMergingIntoSourceFileThrows()
    {
        CreateSourceFiles();

        var options = CreateOptions(onlyIfModified: false);
        options.TargetFile = FirstFile;

        Assert.Throws<InvalidOperationException>(() => FileHelper.MergeFiles(options));
        Assert.That(File.ReadAllText(FirstFile), Is.EqualTo("FIRST\n"));
    }

    /// <summary>
    /// Creates the two source files and sets their timestamps to a defined point in the past
    /// </summary>
    private void CreateSourceFiles()
    {
        File.WriteAllText(FirstFile, "FIRST\n");
        File.WriteAllText(SecondFile, "SECOND\n");
        File.SetLastWriteTimeUtc(FirstFile, Past);
        File.SetLastWriteTimeUtc(SecondFile, Past);
    }

    /// <summary>
    /// Creates an already existing target file with the given timestamp
    /// </summary>
    /// <param name="lastWriteTimeUtc">Timestamp being set for the target file</param>
    private void CreateTargetFile(DateTime lastWriteTimeUtc)
    {
        File.WriteAllText(TargetFile, "OLD TARGET");
        File.SetLastWriteTimeUtc(TargetFile, lastWriteTimeUtc);
    }

    /// <summary>
    /// Creates the options for the merge of the test files
    /// </summary>
    /// <param name="onlyIfModified">true, if the merge shall only be performed for an outdated target</param>
    /// <returns>The created options</returns>
    private FileMergeOptions CreateOptions(bool onlyIfModified)
    {
        return new FileMergeOptions
        {
            FirstFile = FirstFile,
            SecondFile = SecondFile,
            TargetFile = TargetFile,
            PrefixTextTarget = "PREFIX\n",
            OnlyIfModified = onlyIfModified
        };
    }
}
