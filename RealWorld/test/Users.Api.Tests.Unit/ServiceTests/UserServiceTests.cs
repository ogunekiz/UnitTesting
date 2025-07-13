using FluentAssertions;
using FluentValidation;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Users.Api.DTOs;
using Users.Api.Logging;
using Users.Api.Models;
using Users.Api.Repositories;
using Users.Api.Services;

namespace Users.Api.Tests.Unit.ServiceTests
{
    public class UserServiceTests
    {
        private readonly UserService _sut;
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly ILoggerAdapter<UserService> _logger = Substitute.For<ILoggerAdapter<UserService>>();

        public UserServiceTests()
        {
            _sut = new(_userRepository, _logger);
        }

        #region GetAllAsync Tests
        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            _userRepository.GetAllAsync().Returns(Enumerable.Empty<User>().ToList());

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnUsers_WhenUsersExist()
        {
            // Arrange
            var ogunUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Ogun EKIZ"
            };
            var expectedUsers = new List<User>()
            {
              ogunUser
            };
            _userRepository.GetAllAsync().Returns(expectedUsers);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedUsers);

        }

        [Fact]
        public async Task GetAllAsync_ShouldLogMessages_WhenInvoked()
        {
            // Arrange
            _userRepository.GetAllAsync().Returns(Enumerable.Empty<User>().ToList());

            // Act
            await _sut.GetAllAsync();

            // Assert
            _logger.Received(1).LogInformation(Arg.Is("Retrieving all users"));
            _logger.Received(1).LogInformation(Arg.Is("All users retrieved in {0}ms"), Arg.Any<long>());
        }

        [Fact]
        public async Task GetAllAsync_ShouldLogMessageAndException_WhenExceptionIsThrown()
        {
            // Arrange
            var exception = new ArgumentException("Something went wrong while retrieving all users");
            _userRepository.GetAllAsync().Throws(exception);

            // Act
            var requestAction = async () => await _sut.GetAllAsync();

            // Assert
            await requestAction.Should()
                .ThrowAsync<ArgumentException>();
            _logger.Received(1).LogError(Arg.Is(exception), Arg.Is("Something went wrong while retrieving all users"));
        }

        #endregion

        #region GetByIdAsync Tests
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNoUsersExist()
        {
            // Arrange
            _userRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

            // Act
            var result = await _sut.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUsers_WhenUsersExist()
        {
            // Arrange
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Ogun EKIZ"
            };

            _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(existingUser);

            // Act
            var result = await _sut.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeEquivalentTo(existingUser);

        }

        [Fact]
        public async Task GetByIdAsync_ShouldLogMessages_WhenInvoked()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepository.GetByIdAsync(userId).ReturnsNull();

            // Act
            await _sut.GetByIdAsync(userId);

            // Assert
            _logger.Received(1).LogInformation(Arg.Is("Retrieving user with id: {0}"), userId);
            _logger.Received(1).LogInformation(Arg.Is("User with id: {0} retrieved in {1}ms"), userId, Arg.Any<long>());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldLogMessageAndException_WhenExceptionIsThrown()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var exception = new ArgumentException("Something went wrong while retrieving user");
            _userRepository.GetByIdAsync(userId).Throws(exception);

            // Act
            var requestAction = async () => await _sut.GetByIdAsync(userId);

            // Assert
            await requestAction.Should()
                .ThrowAsync<ArgumentException>();
            _logger.Received(1).LogError(
                Arg.Is(exception),
                Arg.Is("Something went wrong while retrieving with id: {0}"),
                Arg.Is(userId));
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldThrownAnError_WhenUserCreateDetailsAreNotValid()
        {
            // Arrange
            CreateUserDto request = new("");

            // Act
            var action = async () => await _sut.CreateAsync(request);

            // Assert
            action.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateAsync_ShouldThrownAnError_WhenUserNameExist()
        {
            // Arrange
            _userRepository.NameIsExist(Arg.Any<string>()).Returns(true);

            // Act
            var action = async () => await _sut.CreateAsync(new("Ogun EKIZ"));

            // Assert
            action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public void CreateAsync_ShouldCreateUserDtoToUserObject()
        {
            // Arrange
            CreateUserDto request = new("Ogun EKIZ");

            // Act
            var user = _sut.CreateUserDtoToUserObject(request);

            // Assert
            user.FullName.Should().Be(request.FullName);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateUser_WhenDetailsAreValidAndUnique()
        {
            // Arrange
            CreateUserDto request = new("Ogun EKIZ");
            _userRepository.NameIsExist(request.FullName).Returns(false);
            _userRepository.CreateAsync(Arg.Any<User>()).Returns(true);

            // Act
            var result = await _sut.CreateAsync(request);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public async Task CreateAsync_ShouldLogMessages_WhenInvoked()
        {
            // Arrange
            CreateUserDto request = new("Ogun EKIZ");
            _userRepository.NameIsExist(request.FullName).Returns(false);
            _userRepository.CreateAsync(Arg.Any<User>()).Returns(true);

            // Act
            await _sut.CreateAsync(request);

            // Assert
            _logger.Received(1).LogInformation(
                Arg.Is("Creating user with id: {0} and name: {1}"),
                Arg.Any<Guid>(),
                Arg.Is(request.FullName));
            _logger.Received(1).LogInformation(
                Arg.Is("User with id: {0} created in {1} ms"),
                Arg.Any<Guid>(),
                Arg.Any<long>());
        }

        [Fact]
        public async Task CreateAsync_ShouldLogMessageAndException_WhenExceptionIsThrown()
        {
            // Arrange
            CreateUserDto request = new("Ogun EKIZ");
            var exception = new ArgumentException("Something went wrong while creating a user");
            _userRepository.CreateAsync(Arg.Any<User>()).Throws(exception);

            // Act
            var requestAction = async () => await _sut.CreateAsync(request);

            // Assert
            await requestAction.Should()
                .ThrowAsync<ArgumentException>();

            _logger.Received(1).LogError(Arg.Is(exception), Arg.Is("Something went wrong while creating a user"));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteByIdAsync_ShouldThrownError_WhenUserNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepository.GetByIdAsync(userId).ReturnsNull();

            // Act
            var action = async () => await _sut.DeleteByIdAsync(userId);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeleteByIdAsync_ShouldDeleteUser_WhenUserExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            User user = new()
            {
                Id = userId,
                FullName = "Ogun EKIZ"
            };
            _userRepository.GetByIdAsync(userId).Returns(user);
            _userRepository.DeleteAsync(user).Returns(true);

            // Act
            var result = await _sut.DeleteByIdAsync(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteByIdAsync_ShouldLogMessages_WhenInvoked()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FullName = "Ogun EKIZ"
            };
            _userRepository.GetByIdAsync(userId).Returns(user);
            _userRepository.DeleteAsync(user).Returns(true);

            // Act
            await _sut.DeleteByIdAsync(userId);

            // Assert
            _logger.Received(1).LogInformation(
                Arg.Is("Deleting user with id: {0}"),
                Arg.Is(userId));
            _logger.Received(1).LogInformation(
                Arg.Is("User with id: {0} deleted in {1} ms"),
                Arg.Is(userId),
                Arg.Any<long>());
        }

        [Fact]
        public async Task DeleteByIdAsync_ShouldLogMessageAndException_WhenExceptionIsThrown()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FullName = "Ogun EKIZ"
            };

            _userRepository.GetByIdAsync(userId).Returns(user);
            var exception = new ArgumentException("Something went wrong while deleting user");
            _userRepository.DeleteAsync(user).Throws(exception);

            // Act
            var requestAction = async () => await _sut.DeleteByIdAsync(userId);

            // Assert
            await requestAction.Should()
                .ThrowAsync<ArgumentException>();

            _logger.Received(1).LogError(Arg.Is(exception), Arg.Is("Something went wrong while deleting user"));
        }


        #endregion

    }
}
