using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Pondrop.Service.Store.Domain.Events;
using Pondrop.Service.Store.Domain.Events.Store;
using Pondrop.Service.Store.Domain.Events.User;

namespace Pondrop.Service.Store.Domain.Models;

public record StoreAddressRecord(
    Guid Id,
    string ExternalReferenceId,
    string AddressLine1,
    string AddressLine2,
    string Suburb,
    string State,
    string Postcode,
    string Country,
    double Latitude,
    double Longitude,
    string CreatedBy,
    string UpdatedBy,
    DateTime CreatedUtc,
    DateTime UpdatedUtc) 
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc);
    