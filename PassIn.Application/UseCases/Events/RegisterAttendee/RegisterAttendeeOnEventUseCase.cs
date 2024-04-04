using Microsoft.EntityFrameworkCore;
using PassIn.Communication.Requests;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;
using System.Net.Mail;

namespace PassIn.Application.UseCases.Events.RegisterAttendee;
public class RegisterAttendeeOnEventUseCase
{
    private readonly PassInDbContext _dbContext;
    public RegisterAttendeeOnEventUseCase()
    {
        _dbContext = new PassInDbContext();
    }
    public ResponseRegisteredJson Execute(Guid eventId, RequestRegisterEventJson request)
    {

        Validate(eventId, request);
        var entity = new Infrastructure.Entities.Attendee
        {
            Name = request.Name,
            Email = request.Email,
            Created_At = DateTime.UtcNow,
            Event_Id = eventId,
        };

        _dbContext.Attendees.Add(entity);
        _dbContext.SaveChanges();

        return new ResponseRegisteredJson
        {
            Id = entity.Id,
        };
    }

    private void Validate(Guid eventId, RequestRegisterEventJson request)
    {
        var eventEntity = _dbContext.Events.Find(eventId);
        if (eventEntity is null)

            throw new NotFoundException("An evente with this id does not exist.");
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ErrorOnValidationException("The name is invalid.");
        }

        if (EmailIsValid(request.Email) == false)
        {
            throw new ErrorOnValidationException("This e-mail is invalid");
        }

        var attendeeIsAlreadyRegister = _dbContext.Attendees.Any(attendee => attendee.Email.Equals(request.Email) && attendee.Event_Id == eventId);

        if (attendeeIsAlreadyRegister)
        {
            throw new ErrorOnValidationException("You can not register twice on the same event.");
        }

        var attendeesForEvent = _dbContext.Attendees.Count(attendees => attendees.Event_Id ==  eventId);
        if(attendeesForEvent == eventEntity.Maximum_Attendees)
        {
            throw new ErrorOnValidationException("There is no room for this event. ");
        }
    }

    private bool EmailIsValid(string email)
    {
        try
        {
            new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
