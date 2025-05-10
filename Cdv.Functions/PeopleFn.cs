using Cdv.Domain.DbContext;
using Cdv.Functions.Dto;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Linq;
using Cdv.Domain.Entities;

namespace Cdv.Functions;

public class PeopleFn
{
    private readonly ILogger<PeopleFn> _logger;
    private readonly PeopleDbContext db;

    public PeopleFn(ILogger<PeopleFn> logger, PeopleDbContext db)
    {
        _logger = logger;
        this.db = db;
    }

    [Function("PeopleFn")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete")] HttpRequest req)
    {
        switch (req.Method)
        {
            case "POST":
                var person = CreatePerson(req);
                return new OkObjectResult(person);
            case "GET":
                var idExist = req.Query.Any(w => w.Key == "id");
                if (idExist)
                {
                    var personId = req.Query.First(w => w.Key == "id").Value;
                    int id = Int32.Parse(personId.First());
                    var foundPerson = FindPerson(id);
                    if (foundPerson != null)
                    {
                        return new OkObjectResult(foundPerson);
                    }
                    return new NotFoundResult();
                }
                var people = GetPeople(req);
                return new OkObjectResult(people);

            case "DELETE":
                DeletePerson(req);
                return new OkResult();

            case "PUT":
                UpdatePerson(req);
                return new OkResult();
        }

        throw new NotImplementedException("Unknown method");
    }

    private List<PersonDto> GetPeople(HttpRequest req)
    {
        var people = db.People.ToList();
        return people.Select(s => new PersonDto
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
        }).ToList();
    }

    private void UpdatePerson(HttpRequest req)
    {
        string requestBody = new StreamReader(req.Body).ReadToEnd();
        var updatedPersonDto = JsonSerializer.Deserialize<PersonDto>(requestBody);
        if (updatedPersonDto == null)
        {
            return;
        }
        var personToUpdate = db.People.FirstOrDefault(p => p.Id == updatedPersonDto.Id);
        if (personToUpdate != null)
        {
            personToUpdate.FirstName = updatedPersonDto.FirstName;
            personToUpdate.LastName = updatedPersonDto.LastName;

            db.SaveChanges();
        }
    }

    private void DeletePerson(HttpRequest req)
    {
        var personId = req.Query["id"].ToString();
        if (int.TryParse(personId, out int id))
        {
            var personToDelete = db.People.FirstOrDefault(p => p.Id == id);
            if (personToDelete != null)
            {
                db.People.Remove(personToDelete);
                db.SaveChanges();
            }
        }
    }

    private PersonDto? FindPerson(int personId)
    {
        var person = db.People.FirstOrDefault(w => w.Id == personId);
        if (person==null)
        {
            return null;
        }
        return new PersonDto
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
        };
    }
    private async Task<CreatePersonDto?> CreatePerson(HttpRequest req)
    {
        try
        {
            // Read the request body asynchronously
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Log the request body for debugging (optional)
            _logger.LogInformation("Request body: {RequestBody}", requestBody);

            // Deserialize the request body to a DTO
            var newPersonDto = JsonSerializer.Deserialize<CreatePersonDto>(requestBody);

            // If the deserialization failed or the DTO is null, return null
            if (newPersonDto == null)
            {
                _logger.LogError("Deserialization failed. Request body might be malformed.");
                return null;
            }

            // Create a new PersonEntity based on the deserialized data
            var newPerson = new PersonEntity
            {
                FirstName = newPersonDto.FirstName,
                LastName = newPersonDto.LastName,
            };

            // Log the person being added to the database (optional)
            _logger.LogInformation("Adding new person: {FirstName} {LastName}", newPerson.FirstName, newPerson.LastName);

            // Add to the database and save changes asynchronously
            db.People.Add(newPerson);
            await db.SaveChangesAsync(); // Use async for non-blocking DB calls

            // Return the DTO as the response
            return newPersonDto;
        }
        catch (JsonException jsonEx)
        {
            // Log JSON deserialization errors
            _logger.LogError(jsonEx, "Error deserializing the request body");
            return null;
        }
        catch (Exception ex)
        {
            // Log any other exceptions
            _logger.LogError(ex, "An error occurred while creating a new person");
            return null;
        }
    }
}
