using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SmartGearOnline.Controllers;
using SmartGearOnline.Models;
using SmartGearOnline.Services;
using Xunit;

namespace SmartGearOnline.Tests
{
    public class ProductControllerTests
    {
        private Mock<IProductService> CreateServiceMock() => new();
        private Mock<ILogger<ProductController>> CreateLoggerMock() => new();

        private static void VerifyLogError(Mock<ILogger<ProductController>> loggerMock, Times times)
        {
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>() ),
                times);
        }

        private static void VerifyLogInformation(Mock<ILogger<ProductController>> loggerMock, Times times)
        {
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>() ),
                times);
        }

        [Fact]
        public async Task Index_ReturnsView_WithProductList()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            var fake = new List<Product> { new Product { Id = 1, Name = "A", BasePrice = 1, MarkupPercentage = 0, CategoryId = 1 } };
            svc.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(fake);

            var controller = new ProductController(svc.Object, log.Object);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(fake, view.Model);
        }

        [Fact]
        public async Task Index_ServiceThrows_LogsAndReturns500()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            svc.Setup(s => s.GetAllProductsAsync()).ThrowsAsync(new Exception("boom"));

            var controller = new ProductController(svc.Object, log.Object);

            var result = await controller.Index();

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            VerifyLogError(log, Times.Once());
        }

        [Fact]
        public async Task Details_ProductExists_ReturnsView()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            var p = new Product { Id = 10, Name = "P", BasePrice = 10, MarkupPercentage = 5, CategoryId = 1 };
            svc.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(p);

            var controller = new ProductController(svc.Object, log.Object);

            var result = await controller.Details(10);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(p, view.Model);
        }

        [Fact]
        public async Task Details_NotFound_ReturnsNotFound()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            svc.Setup(s => s.GetByIdAsync(5)).ReturnsAsync((Product?)null);

            var controller = new ProductController(svc.Object, log.Object);

            var result = await controller.Details(5);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_Post_ValidProduct_RedirectsToIndex_AndLogs()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            var product = new Product { Id = 1, Name = "New", BasePrice = 100, MarkupPercentage = 10, CategoryId = 1 };
            svc.Setup(s => s.AddProductAsync(product)).Returns(Task.CompletedTask);

            var controller = new ProductController(svc.Object, log.Object);

            var result = await controller.Create(product, "admin");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            // avoid nullable dereference warnings
            Assert.NotNull(redirect.RouteValues);
            var rv = redirect.RouteValues!;
            Assert.True(rv.ContainsKey("user"));
            Assert.Equal("admin", rv["user"]);

            VerifyLogInformation(log, Times.Once());
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithoutCallingService()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            // ensure we return the correct category type expected by the service
            svc.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<Category>());
            var controller = new ProductController(svc.Object, log.Object);
            controller.ModelState.AddModelError("Name", "Required");

            var product = new Product { Id = 1, Name = "", BasePrice = 0, MarkupPercentage = 0, CategoryId = 1 };

            var result = await controller.Create(product, "admin");

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(product, view.Model);
            svc.Verify(s => s.AddProductAsync(It.IsAny<Product>()), Times.Never());
        }

        [Fact]
        public async Task Edit_Post_UpdateSuccess_Redirects()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            svc.Setup(s => s.UpdateProductAsync(2, It.IsAny<Product>())).ReturnsAsync(true);

            var controller = new ProductController(svc.Object, log.Object);
            var updated = new Product { Id = 2, Name = "U", BasePrice = 5, MarkupPercentage = 1, CategoryId = 1 };

            var result = await controller.Edit(2, updated, "user");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            VerifyLogInformation(log, Times.Once());
        }

        [Fact]
        public async Task Edit_Post_UpdateFails_ReturnsNotFound()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            svc.Setup(s => s.UpdateProductAsync(3, It.IsAny<Product>())).ReturnsAsync(false);

            var controller = new ProductController(svc.Object, log.Object);
            var updated = new Product { Id = 3, Name = "X", BasePrice = 1, MarkupPercentage = 1, CategoryId = 1 };

            var result = await controller.Edit(3, updated, "user");

            Assert.IsType<NotFoundResult>(result);
            VerifyLogInformation(log, Times.Never());
        }

        [Fact]
        public async Task DeleteConfirmed_Success_RedirectsAndLogs()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            svc.Setup(s => s.DeleteProductAsync(7)).ReturnsAsync(true);

            var controller = new ProductController(svc.Object, log.Object);

            var result = await controller.DeleteConfirmed(7, "admin");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            VerifyLogInformation(log, Times.Once());
        }

        [Fact]
        public async Task DeleteConfirmed_Failure_ReturnsNotFound()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            svc.Setup(s => s.DeleteProductAsync(8)).ReturnsAsync(false);

            var controller = new ProductController(svc.Object, log.Object);

            var result = await controller.DeleteConfirmed(8, "admin");

            Assert.IsType<NotFoundResult>(result);
            VerifyLogInformation(log, Times.Never());
        }

        [Fact]
        public async Task DeleteConfirmed_ServiceThrows_LogsAndReturns500()
        {
            var svc = CreateServiceMock();
            var log = CreateLoggerMock();

            svc.Setup(s => s.DeleteProductAsync(9)).ThrowsAsync(new Exception("boom"));

            var controller = new ProductController(svc.Object, log.Object);

            var result = await controller.DeleteConfirmed(9, "admin");

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            VerifyLogError(log, Times.Once());
        }
    }
}
