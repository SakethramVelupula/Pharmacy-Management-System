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
                new DrugDto { DrugId = 1, Name = "Paracetamol", Manufacturer = "ABC Pharma", Price = 10, Stock = 100 },
                new DrugDto { DrugId = 2, Name = "Ibuprofen", Manufacturer = "XYZ Pharma", Price = 15, Stock = 50 }
            };
            _mockDrugService.Setup(s => s.GetAllDrugsAsync()).ReturnsAsync(drugList);

            var result = await _controller.GetAllDrugs();

            var okResult = result as OkObjectResult;
            ClassicAssert.IsNotNull(okResult);
            ClassicAssert.AreEqual(200, okResult.StatusCode);

            var returnedDrugs = okResult.Value as IEnumerable<DrugDto>;
            ClassicAssert.AreEqual(2, ((List<DrugDto>)returnedDrugs).Count);
        }

        [Test]
        public async Task GetDrugById_ReturnsOk_WhenDrugExists()
        {
            var drug = new DrugDto { DrugId = 1, Name = "Paracetamol", Manufacturer = "ABC Pharma", Price = 10, Stock = 100 };
            _mockDrugService.Setup(s => s.GetDrugByIdAsync(1)).ReturnsAsync(drug);

            var result = await _controller.GetDrugById(1);

            var okResult = result as OkObjectResult;
            ClassicAssert.IsNotNull(okResult);
            var returnedDrug = okResult.Value as DrugDto;
            ClassicAssert.AreEqual("Paracetamol", returnedDrug.Name);
        }

        [Test]
        public async Task GetDrugById_ReturnsNotFound_WhenDrugDoesNotExist()
        {
            _mockDrugService.Setup(s => s.GetDrugByIdAsync(It.IsAny<int>())).ReturnsAsync((DrugDto)null);

            var result = await _controller.GetDrugById(99);
            var objectResult = result as ObjectResult;
            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(404, objectResult.StatusCode);
            StringAssert.Contains("not found", objectResult.Value?.ToString());


        }

        [Test]
        public async Task CreateDrug_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var createDto = new CreateDrugDto { Name = "Amoxicillin", Manufacturer = "HealthCorp", Price = 25 };
            var createdDrug = new DrugDto { DrugId = 3, Name = "Amoxicillin", Manufacturer = "HealthCorp", Price = 25, Stock = 200 };

            _mockDrugService.Setup(s => s.AddDrugAsync(createDto)).ReturnsAsync(createdDrug);

            var result = await _controller.AddDrug(createDto);

            var createdResult = result as CreatedAtActionResult;
            ClassicAssert.IsNotNull(createdResult);
            ClassicAssert.AreEqual("GetDrugById", createdResult.ActionName);

            var returnedDrug = createdResult.Value as DrugDto;
            ClassicAssert.AreEqual("Amoxicillin", returnedDrug.Name);
        }

        [Test]
        public async Task DeleteDrug_ReturnsOk_WhenSuccessful()
        {
            _mockDrugService.Setup(s => s.DeleteDrugAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteDrug(1);
            var okResult = result as OkObjectResult;
            ClassicAssert.IsNotNull(okResult);
            ClassicAssert.AreEqual(200, okResult.StatusCode);
            StringAssert.Contains("deleted successfully", okResult.Value?.ToString());
        }

        [Test]
        public async Task DeleteDrug_ReturnsNotFound_WhenDrugDoesNotExist()
        {
            _mockDrugService.Setup(s => s.DeleteDrugAsync(1)).ReturnsAsync(false);
            var result = await _controller.DeleteDrug(1);
            var objectResult = result as ObjectResult;
            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(404, objectResult.StatusCode);
            StringAssert.Contains("not found", objectResult.Value?.ToString());
        }
    }
}