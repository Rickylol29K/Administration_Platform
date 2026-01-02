using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministrationPlatformTesting.Logic;

[TestClass]
public class ApplicationLogicTests
{
    [TestMethod]
    public void Constructor_WithNullRepository_Throws()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new ApplicationLogic(null!));
    }
}
