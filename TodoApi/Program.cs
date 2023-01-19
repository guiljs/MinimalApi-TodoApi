using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => Results.Ok("Ok ok"));

app.MapGet("/todoitems", async (TodoDb db) => await db.Todos.ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Where(x => x.IsComplete).ToListAsync());

app.MapGet("/todoitems/complete2", async (TodoDb db) =>
{
    var result = await db.Todos.Where(x => x.IsComplete).ToListAsync();

    return result;
});

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound()
);

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo todo, TodoDb db) =>
{
    var todoOriginal = await db.Todos.FindAsync(id);

    if (todoOriginal is null) return Results.NotFound();

    todoOriginal.Name = todo.Name;
    todoOriginal.IsComplete = todo.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();

});

app.MapDelete("/todoitems/{id}", async (TodoDb db, int id) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

app.Run();