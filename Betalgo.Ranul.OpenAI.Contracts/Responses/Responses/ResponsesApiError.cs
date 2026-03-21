using System.Text.Json.Serialization;
using Betalgo.Ranul.OpenAI.Contracts.Enums.Responses;

namespace Betalgo.Ranul.OpenAI.Contracts.Responses.Responses;

/// <summary>
///     An error object returned when the model fails to generate a Response.
///     This is specific to the Responses API and different from the general ResponseError.
///     <see href="https://platform.openai.com/docs/api-reference/responses/object">OpenAI API documentation</see>.
///     <see href="https://github.com/betalgo/openai/blob/master/Docs/openapi-split/components/schemas/responseerror.yml">
///         Source Definition
///     </see>
/// </summary>
public class ResponsesApiError
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ResponsesApiError" /> class.
    /// </summary>
    public ResponsesApiError()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResponsesApiError" /> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public ResponsesApiError(ResponsesErrorCode code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    ///     The error code for the response.
    /// </summary>
    [JsonPropertyName("code")]
    public ResponsesErrorCode? Code { get; set; }

    /// <summary>
    ///     A human-readable description of the error.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

