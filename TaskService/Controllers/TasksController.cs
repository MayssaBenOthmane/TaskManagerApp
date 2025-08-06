using Microsoft.AspNetCore.Mvc;
using TaskService.Models;
using System.Text.Json;
using TaskService.Interfaces;

namespace TaskService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository repo;
    private readonly IRabbitMQPublisher publisher;

    public TasksController(ITaskRepository repo, IRabbitMQPublisher publisher)
    {
        this.repo = repo;
        this.publisher = publisher;
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
