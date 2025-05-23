using CalculatorLibrary;
using FluentAssertions;

namespace CalculatorLibraryTests
{
    public class CalculatorTests
    {
        private readonly Calculator _sut = new();

        [Theory]
        [InlineData(5, 5, 10)]
        [InlineData(-5, 5, 0)]
        [InlineData(-15, -5, -20)]
        public void Add_ShouldAddTwoNumbers_WhenTwoNumbersAreIntegers(int a, int b, int expected)
        {
            //Act
            var result = _sut.Add(a, b);

            //Assers
            //Assert.Equal(expected, result);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(5, -5, 10)]
        [InlineData(15, -5, 20)]
        [InlineData(-5, -5, 0)]
        [InlineData(-15, -5, -10)]
        [InlineData(5, -10, 15)]
        public void Subtract_ShouldSubtractTwoNumbers_WhenTwoNumbersAreIntegers(int a, int b, int expected)
        {
            //Act
            var result = _sut.Subtract(a, b);

            //Assers
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(5, 5, 25)]
        [InlineData(50, 0, 0)]
        [InlineData(-5, 5, -25)]
        public void Multiply_ShouldMultiplyTwoNumbers_WhenTwoNumbersAreIntegers(int a, int b, int expected)
        {
            //Act
            var result = _sut.Multiply(a, b);

            //Assers
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(5, 5, 1)]
        [InlineData(15, 5, 3)]
        [InlineData(0, 0, 0, Skip = "0/0 bölünemez")]
        public void Divide_ShouldDivideTwoNumbers_WhenTwoNumbersAreIntegers(int a, int b, int expected)
        {
            //Act
            var result = _sut.Divide(a, b);

            //Assers
            result.Should().Be(expected);
        }

    }
}
