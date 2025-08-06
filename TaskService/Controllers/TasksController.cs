using Microsoft.AspNetCore.Mvc;
using TaskService.Models;
using TaskService.Services;
using System.Text.Json;

namespace TaskService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskRepository repo;
    private readonly RabbitMQPublisher publisher;

    public TasksController()
    {
        repo = new TaskRepository();
        publisher = new RabbitMQPublisher();
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(repo.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult Get(Guid id)
    {
        var task = repo.GetById(id);
        return task == null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public IActionResult Create(TaskInfo task)
    {
        var created = repo.Add(task);
        publisher.Publish(JsonSerializer.Serialize(new { action = "created", task }));
        return CreatedAtAction(nameof(Get), new { id = task.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, TaskInfo task)
    {
        var success = repo.Update(id, task);
        if (!success) return NotFound();
        publisher.Publish(JsonSerializer.Serialize(new { action = "updated", task }));
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var task = repo.GetById(id);
        if (!repo.Delete(id)) return NotFound();
        publisher.Publish(JsonSerializer.Serialize(new { action = "deleted", task }));
        return NoContent();
    }
}
