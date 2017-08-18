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
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Granikos.SMTPSimulator.Service.ConfigurationService.Models;

namespace Granikos.SMTPSimulator.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/TimeTables")]
    public class TimeTablesController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("Empty")]
        public TimeTable Empty()
        {
            return _service.GetEmptyTimeTable();
        }

        [HttpGet]
        [Route("")]
        public IEnumerable<TimeTable> Get()
        {
            return _service.GetTimeTables();
        }

        [HttpGet]
        [Route("Types")]
        public IEnumerable<NameWithDisplayName> GetTypes()
        {
            return _service.GetTimeTableTypes();
        }

        [HttpGet]
        [Route("MailTemplates")]
        public IEnumerable<object> GetMailTemplates()
        {
            return _service.GetMailTemplates();
        }

        [HttpGet]
        [Route("Attachments")]
        public IEnumerable<Attachment> GetAttachments()
        {
            return _service.GetAttachments();
        }

        [HttpGet]
        [Route("TypeData/{type}")]
        public IDictionary<string, string> GetTypeData(string type)
        {
            return _service.GetTimeTableTypeData(type);
        }

        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            var timeTable = _service.GetTimeTable(id);

            if (timeTable == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find time table.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, timeTable);
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Add([FromBody]TimeTable timeTable)
        {
            var added = _service.AddTimeTable(timeTable);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add time table.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Update(int id, [FromBody]TimeTable timeTable)
        {
            var updated = _service.UpdateTimeTable(timeTable);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update time table.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/LocalUsers/5
        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteTimeTable(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete time table.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}