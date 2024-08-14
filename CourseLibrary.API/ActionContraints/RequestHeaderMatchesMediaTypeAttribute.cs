using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.ActionContraints;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
public class RequestHeaderMatchesMediaTypeAttribute: Attribute, IActionConstraint
{
    private readonly string _requestHeadersToMatch;
    private readonly MediaTypeCollection _mediaTypes = new ();
    public RequestHeaderMatchesMediaTypeAttribute(string requestHeadersToMatch
        , string mediaType, params string[] otherMediaTypes)
    {
        _requestHeadersToMatch = requestHeadersToMatch
            ?? throw new ArgumentNullException(nameof(requestHeadersToMatch));
        if(MediaTypeHeaderValue.TryParse(mediaType, out var mediaTypeHeaderValue))
        {
            _mediaTypes.Add(mediaTypeHeaderValue);
        } else
        {
            throw new ArgumentException(nameof(mediaType));
        }
        foreach(var otherMediaType in otherMediaTypes)
        {
            if(MediaTypeHeaderValue.TryParse(otherMediaType, out var parsedOtherMediaType))
            {
                _mediaTypes.Add(parsedOtherMediaType);
            } else 
            {
                throw new ArgumentException(nameof(otherMediaType));
            }
        }
    }

    public int Order { get; }

    public bool Accept(ActionConstraintContext context)
    {
        var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
        if(!requestHeaders.ContainsKey(_requestHeadersToMatch))
        {
            return false;
        }
        var parseRequestMediaType = new MediaType(requestHeaders[_requestHeadersToMatch]);
        foreach(var mediaType in _mediaTypes)
        {
            var parsedMediaType = new MediaType(mediaType);
            if (parsedMediaType.Equals(parseRequestMediaType))
            {
                return true;
            }
        }
        return false;
    }
}
  