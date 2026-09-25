using csc8208Maui.Services;

namespace csc8208Maui.Tests;

public class UnitTest1
{
    [Fact]
    public void WebService_LoginAsync_Invalid()
    {
        //Arrange
        string username = "_";
        string password = "_";
        //Act
        Task<(bool success, string message)> result = WebService.LoginAsync(username, password);
        //Assert
        
    }
}
