// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Granikos.SMTPSimulator.Service.ConfigurationService.Models;

namespace Granikos.SMTPSimulator.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/ExternalGroups")]
    public class ExternalGroupsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("")]
        public IEnumerable<UserGroup> Get()
        {
            return _service.GetExternalGroups();
        }

        [HttpGet]
        [Route("WithCounts")]
        public object GetWithCounts()
        {
            var total = _service.GetExternalUserCount();

            return new
            {
                Items = _service.GetExternalGroups()
                    .Select(g => new
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Count = g.UserIds.Length
                    }).ToArray(),
                MailboxTotal = total
            };
        }

        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Get(int id)
        {
            var domain = _service.GetExternalGroup(id);

            if (domain == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find external user group.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, domain);
        }

        [HttpPost]
        [Route("{*name}")]
        public HttpResponseMessage Post(string name)
        {
            var added = _service.AddExternalGroup(name);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add external user group.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Put(int id, [FromBody]UserGroup @group)
        {
            var updated = _service.UpdateExternalGroup(@group);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update external user group.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteExternalGroup(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete external user group.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}