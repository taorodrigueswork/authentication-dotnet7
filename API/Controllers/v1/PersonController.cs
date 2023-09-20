using Business.Interfaces;
using Entities.DTO.Request.Person;
using Entities.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

[Authorize]
public class PersonController : BaseController
{
    private readonly IBusiness<PersonDtoRequest, PersonEntity> _personBusiness;

    public PersonController(IBusiness<PersonDtoRequest, PersonEntity> personBusiness)
    {
        _personBusiness = personBusiness;
    }

    /// <summary>
    /// Gets a person by their ID.
    /// </summary>
    /// <param name="id">The ID of the person to get.</param>
    /// <returns>An IActionResult representing the status of the operation with the details of the requested entity.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersonEntity))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPersonByIdAsync(int id)
    {
        var person = await _personBusiness.GetById(id);
        return person == null ? NotFound() : Ok(person);
    }

    /// <summary>
    /// Adds a new person.
    /// </summary>
    /// <param name="personDto">The Person object to add.</param>
    /// <returns>An IActionResult representing the status of the operation with the details of the new entity created.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PersonEntity))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> AddPersonAsync([FromBody] PersonDtoRequest personDto)
    {
        return Created(string.Empty, await _personBusiness.Add(personDto));
    }

    /// <summary>
    /// Updates an existing Person.
    /// </summary>
    /// <param name="id">The ID of the Person to update, passed in a header.</param>
    /// <param name="personDTO">The updated Person object.</param>
    /// <returns>An IActionResult representing the status of the operation with the details of the updated entity.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersonEntity))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdatePersonAsync(int id, [FromBody] PersonDtoRequest personDTO)
    {
        var updatedPerson = await _personBusiness.Update(id, personDTO);

        return updatedPerson == null ? NotFound() : Ok(updatedPerson);
    }
    /// <summary>
    /// Deletes a person with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the person to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePersonAsync(int id)
    {
        await _personBusiness.Delete(id);
        return NoContent();
    }

}