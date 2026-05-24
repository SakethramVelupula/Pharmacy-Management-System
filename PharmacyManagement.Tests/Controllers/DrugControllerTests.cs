using NUnit.Framework.Legacy;
using Moq;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Controllers;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Tests.Controllers
{
    [TestFixture]
    public class DrugControllerTests
    {
        private Mock<IDrugsService> _mockDrugService;
        private DrugsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockDrugService = new Mock<IDrugsService>();
            _controller = new DrugsController(_mockDrugService.Object);
        }

        [Test]
        public async Task GetAllDrugs_ReturnsOk_WithListOfDrugs()
        {
            var drugList = new List<DrugDto>
            {
                new DrugDto { DrugId = 1, Name = "Paracetamol", Manufacturer = "ABC Pharma", PricePerUnit = 10, Stock = 100 },
                new DrugDto { DrugId = 2, Name = "Ibuprofen", Manufacturer = "XYZ Pharma", PricePerUnit = 15, Stock = 50 }
            };
            _mockDrugService.Setup(s => s.GetAllDrugsAsync()).ReturnsAsync(drugList);

            var result = await _controller.GetAllDrugs();

            var okResult = result as OkObjectResult;
            ClassicAssert.IsNotNull(okResult);
            ClassicAssert.AreEqual(200, okResult.StatusCode);
            var returnedDrugs = okResult.Value as List<DrugDto>;
            ClassicAssert.AreEqual(2, returnedDrugs.Count);
        }

        [Test]
        public async Task GetAllDrugs_ReturnsOk_WithEmptyList()
        {
            _mockDrugService.Setup(s => s.GetAllDrugsAsync()).ReturnsAsync(new List<DrugDto>());

            var result = await _controller.GetAllDrugs();

            var okResult = result as OkObjectResult;
            ClassicAssert.IsNotNull(okResult);
            ClassicAssert.AreEqual(200, okResult.StatusCode);
        }

        [Test]
        public async Task GetDrugById_ReturnsOk_WhenDrugExists()
        {
            var drug = new DrugDto { DrugId = 1, Name = "Paracetamol", Manufacturer = "ABC Pharma", PricePerUnit = 10, Stock = 100 };
            _mockDrugService.Setup(s => s.GetDrugByIdAsync(1)).ReturnsAsync(drug);

            var result = await _controller.GetDrugById(1);

            var okResult = result as OkObjectResult;
            ClassicAssert.IsNotNull(okResult);
            ClassicAssert.AreEqual(200, okResult.StatusCode);
            var returnedDrug = okResult.Value as DrugDto;
            ClassicAssert.AreEqual("Paracetamol", returnedDrug.Name);
        }

        [Test]
        public async Task GetDrugById_ReturnsNotFound_WhenDrugDoesNotExist()
        {
            _mockDrugService.Setup(s => s.GetDrugByIdAsync(It.IsAny<int>())).ReturnsAsync((DrugDto)null);

            var result = await _controller.GetDrugById(99);

            var notFoundResult = result as NotFoundObjectResult;
            ClassicAssert.IsNotNull(notFoundResult);
            ClassicAssert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task CreateDrug_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var createDto = new CreateDrugDto { Name = "Amoxicillin", Manufacturer = "HealthCorp", PricePerUnit = 25 };
            var createdDrug = new DrugDto { DrugId = 3, Name = "Amoxicillin", Manufacturer = "HealthCorp", PricePerUnit = 25, Stock = 0 };
            _mockDrugService.Setup(s => s.AddDrugAsync(createDto)).ReturnsAsync(createdDrug);

            var result = await _controller.AddDrug(createDto);

            var createdResult = result as CreatedAtActionResult;
            ClassicAssert.IsNotNull(createdResult);
            ClassicAssert.AreEqual(201, createdResult.StatusCode);
            ClassicAssert.AreEqual("GetDrugById", createdResult.ActionName);
            var returnedDrug = createdResult.Value as DrugDto;
            ClassicAssert.AreEqual("Amoxicillin", returnedDrug.Name);
        }

        [Test]
        public async Task UpdateDrug_ReturnsOk_WhenDrugExists()
        {
            var updateDto = new UpdateDrugDto { Name = "Paracetamol", Manufacturer = "ABC Pharma", PricePerUnit = 12 };
            var updatedDrug = new DrugDto { DrugId = 1, Name = "Paracetamol", Manufacturer = "ABC Pharma", PricePerUnit = 12, Stock = 100 };
            _mockDrugService.Setup(s => s.UpdateDrugAsync(1, updateDto)).ReturnsAsync(updatedDrug);

            var result = await _controller.UpdateDrug(1, updateDto);

            var okResult = result as OkObjectResult;
            ClassicAssert.IsNotNull(okResult);
            ClassicAssert.AreEqual(200, okResult.StatusCode);
            var returnedDrug = okResult.Value as DrugDto;
            ClassicAssert.AreEqual(12, returnedDrug.PricePerUnit);
        }

        [Test]
        public async Task UpdateDrug_ReturnsNotFound_WhenDrugDoesNotExist()
        {
            var updateDto = new UpdateDrugDto { Name = "Ghost", Manufacturer = "None", PricePerUnit = 1 };
            _mockDrugService.Setup(s => s.UpdateDrugAsync(99, updateDto)).ReturnsAsync((DrugDto)null);

            var result = await _controller.UpdateDrug(99, updateDto);

            var notFoundResult = result as NotFoundObjectResult;
            ClassicAssert.IsNotNull(notFoundResult);
            ClassicAssert.AreEqual(404, notFoundResult.StatusCode);
        }

        [Test]
        public async Task GetLowStockDrugs_ReturnsOk_WithLowStockList()
        {
            var lowStockDrugs = new List<DrugDto>
            {
                new DrugDto { DrugId = 1, Name = "Paracetamol", Stock = 5, LowStockThreshold = 10 }
            };
            _mockDrugService.Setup(s => s.GetLowStockDrugsAsync()).ReturnsAsync(lowStockDrugs);

            var result = await _controller.GetLowStockDrugs();

            var okResult = result as OkObjectResult;
            ClassicAssert.IsNotNull(okResult);
            ClassicAssert.AreEqual(200, okResult.StatusCode);
        }
    }
}
