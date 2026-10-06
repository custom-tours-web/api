# api

Core backend service for the travel platform, providing endpoints for location search, reservation management, and third-party travel supplier integrations.

## Booking notification feature flags

WhatsApp and email booking notifications are independently controlled by `BookingNotifications:WhatsAppEnabled` and `BookingNotifications:EmailEnabled`. Both flags are `false` by default in `appsettings.json`, so no external notifications are sent unless explicitly enabled through configuration (for example, `BookingNotifications__WhatsAppEnabled=true`).

## Booking WhatsApp notifications

When `BookingNotifications:WhatsAppEnabled` is `true`, after a booking is saved the API sends its details to `MetaWhatsApp:WhatsAppTo` through Meta's WhatsApp Cloud API using an approved message template. The destination is currently `+918428558275`.

Set these values through environment variables or a secret store; do not commit access tokens:

- `MetaWhatsApp__AccessToken` — a Meta access token with permission to send WhatsApp messages.
- `MetaWhatsApp__PhoneNumberId` — the WhatsApp Business phone number ID from Meta.
- `MetaWhatsApp__GraphApiVersion` — currently set to `v26.0`; update this when Meta retires that Graph API version.
- `MetaWhatsApp__TemplateName` — currently set to `new_booking_request`; this must match the approved booking-alert template name.
- `MetaWhatsApp__TemplateLanguageCode` — currently set to `en_US`; this must match the locale approved for the template.

Create and approve a utility template whose body has these nine text placeholders, in this exact order:

```text
New booking request #{{1}}
Name: {{2}}
Phone: {{3}}
From: {{4}}
Destination: {{5}}
Dates: {{6}} to {{7}}
Members: {{8}}
Special requests: {{9}}
```

The recipient must have opted in to receive messages from the business. Template messages can be sent outside the 24-hour customer-service window, but utility templates sent outside that window are chargeable under Meta's current per-message pricing. Meta's current pricing documentation says non-template messages are free only within an open customer-service window; it does not specify a general allowance of 1,000 free service conversations per month. If Meta is not configured or delivery fails, the booking remains saved and the failure is logged.

## Booking email notifications

When `BookingNotifications:EmailEnabled` is `true`, each saved booking is also sent by SMTP to `naren000000000@gmail.com`, concurrently with the WhatsApp notification if that feature is enabled. Configure the following settings through environment variables or a secret store; do not commit SMTP credentials:

- `BookingEmail__SmtpHost`
- `BookingEmail__SmtpPort` (defaults to `587`)
- `BookingEmail__SmtpUsername`
- `BookingEmail__SmtpPassword`
- `BookingEmail__FromAddress`
- `BookingEmail__FromName` (defaults to `Custom Tours`)

SMTP uses TLS. For Gmail SMTP, use a Google app password if required by the account's security settings. If email settings are incomplete or delivery fails, the booking remains saved and WhatsApp delivery proceeds independently; failures are logged.

## OpenAPI contract tests

The integration suite verifies that the generated OpenAPI document describes the booking and health endpoints, and that valid and invalid booking requests produce responses matching that contract. Run these checks with:

```sh
dotnet test tests/it/it.csproj --filter FullyQualifiedName~OpenApiContractTests
```
