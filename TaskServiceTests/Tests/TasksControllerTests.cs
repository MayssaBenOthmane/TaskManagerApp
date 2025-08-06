using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TaskService.Controllers;
using TaskService.Interfaces;
using TaskService.Models;

namespace TaskServiceTests.Tests
{
    public class TasksControllerTests
    {
        private readonly ITaskRepository repo;
        private readonly IRabbitMQPublisher publisher;
        private readonly TasksController controller;

        public TasksControllerTests()
        {
            repo = Substitute.For<ITaskRepository>();
            publisher = Substitute.For<IRabbitMQPublisher>();
            controller = new TasksController(repo, publisher);
        }

        [Fact]
        public void GetAll_ReturnsAllTasks()
        {
            //Arrange
            var tasks = new List<TaskInfo>
            {
                new TaskInfo { Title = "Task1" },
                new TaskInfo { Title = "Task2" }
            };
            repo.GetAll().Returns(tasks);

            //Act
            var result = controller.GetAll();

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTasks = Assert.IsAssignableFrom<IEnumerable<TaskInfo>>(okResult.Value);
            Assert.Equal(2, ((List<TaskInfo>)returnedTasks).Count);
        }

        [Fact]
        public void Get_ReturnsTask_WhenFound()
        {
            //Arrange
            var task = new TaskInfo { Id = Guid.NewGuid(), Title = "Task1" };
            repo.GetById(task.Id).Returns(task);

            //Act
            var result = controller.Get(task.Id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTask = Assert.IsType<TaskInfo>(okResult.Value);
            Assert.Equal(task.Id, returnedTask.Id);
        }

        [Fact]
        public void Get_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            //Arrange
            var id = Guid.NewGuid();
            repo.GetById(id).Returns((TaskInfo)null);

            //Act
            var result = controller.Get(id);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_AddsTask_AndPublishesEvent()
        {
            //Arrange
            var task = new TaskInfo { Id = Guid.NewGuid(), Title = "New Task" };
            repo.Add(task).Returns(task);

            //Act
            var result = controller.Create(task);

            //Assert
            repo.Received(1).Add(task);
            publisher.Received(1).Publish(Arg.Is<string>(msg => msg.Contains("created") && msg.Contains(task.Title)));

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedTask = Assert.IsType<TaskInfo>(createdResult.Value);
            Assert.Equal(task.Id, returnedTask.Id);
        }

        [Fact]
        public void Update_ReturnsNoContent_WhenSuccessful()
        {
            //Arrange
            var id = Guid.NewGuid();
            var task = new TaskInfo { Title = "Updated Task" };
            repo.Update(id, task).Returns(true);

            //Act
            var result = controller.Update(id, task);

            //Assert
            repo.Received(1).Update(id, task);
            publisher.Received(1).Publish(Arg.Is<string>(msg => msg.Contains("updated") && msg.Contains(task.Title)));
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Update_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            //Arrange
            var id = Guid.NewGuid();
            var task = new TaskInfo();
            repo.Update(id, task).Returns(false);

            //Act
            var result = controller.Update(id, task);

            //Assert
            Assert.IsType<NotFoundResult>(result);
            publisher.DidNotReceive().Publish(Arg.Any<string>());
        }
        [Fact]
        public void Delete_ReturnsNoContent_WhenSuccessful()
        {
            //Arrange
            var id = Guid.NewGuid();
            var task = new TaskInfo { Id = id, Title = "To be deleted" };
            repo.GetById(id).Returns(task);
            repo.Delete(id).Returns(true);

            //Act
            var result = controller.Delete(id);

            //Assert
            repo.Received(1).Delete(id);
            publisher.Received(1).Publish(Arg.Is<string>(msg => msg.Contains("deleted") && msg.Contains(task.Title)));
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            //Arrange
            var id = Guid.NewGuid();
            repo.GetById(id).Returns((TaskInfo)null);
            repo.Delete(id).Returns(false);

            // Act
            var result = controller.Delete(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            publisher.DidNotReceive().Publish(Arg.Any<string>());
        }
    }
}
