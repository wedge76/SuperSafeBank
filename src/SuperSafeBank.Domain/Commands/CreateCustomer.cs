using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SuperSafeBank.Common;
using SuperSafeBank.Common.EventBus;
using SuperSafeBank.Domain.IntegrationEvents;
using SuperSafeBank.Domain.Services;

namespace SuperSafeBank.Domain.Commands;

public record CreateCustomer(Guid CustomerId,
                             string FirstName,
                             string LastName,
                             string Email) : IRequest;

public class CreateCustomerHandler : IRequestHandler<CreateCustomer>
{
    private readonly ICustomerEmailsService _customerEmailsRepository;
    private readonly IEventProducer _eventProducer;
    private readonly IAggregateRepository<Customer, Guid> _eventsService;

    public CreateCustomerHandler(IAggregateRepository<Customer, Guid> eventsService,
                                 ICustomerEmailsService customerEmailsRepository,
                                 IEventProducer eventProducer)
    {
        _eventsService = eventsService ?? throw new ArgumentNullException(nameof(eventsService));
        _customerEmailsRepository = customerEmailsRepository ?? throw new ArgumentNullException(nameof(customerEmailsRepository));
        _eventProducer = eventProducer ?? throw new ArgumentNullException(nameof(eventProducer));
    }

    public async Task Handle(CreateCustomer command, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("Invalid email address", new ValidationError(nameof(CreateCustomer.Email), "email cannot be empty"));
        }

        if (await _customerEmailsRepository.ExistsAsync(command.Email))
        {
            throw new ValidationException("Duplicate email address", new ValidationError(nameof(CreateCustomer.Email), $"email '{command.Email}' already exists"));
        }

        var customer = Customer.Create(command.CustomerId,
                                       command.FirstName,
                                       command.LastName,
                                       command.Email);
        await _eventsService.PersistAsync(customer, cancellationToken);
        await _customerEmailsRepository.CreateAsync(command.Email, customer.Id, cancellationToken);

        var @event = new CustomerCreated(Guid.NewGuid(), command.CustomerId);
        await _eventProducer.DispatchAsync(@event, cancellationToken);
    }
}