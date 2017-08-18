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
    [RoutePrefix("api/SendConnectors")]
    public class SendConnectorsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("Empty")]
        public SendConnector Empty()
        {
            return _service.GetEmptySendConnector();
        }

        [HttpGet]
        [Route("Default")]
        public SendConnector Default()
        {
            return _service.GetDefaultSendConnector();
        }

        [HttpPost]
        [Route("Default/{id:int}")]
        public HttpResponseMessage SetDefault(int id)
        {
            if (!_service.SetDefaultSendConnector(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update default send connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // GET api/SendConnectors
        [HttpGet]
        [Route("")]
        public IEnumerable<SendConnector> Get()
        {
            return _service.GetSendConnectors();
        }

        // GET api/SendConnectors/5
        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            var sendConnector = _service.GetSendConnector(id);

            if (sendConnector == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find send connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, sendConnector);
        }

        // POST api/SendConnectors
        [HttpPost]
        [Route("")]
        public HttpResponseMessage Post([FromBody]SendConnector sendConnector)
        {
            var added = _service.AddSendConnector(sendConnector);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add send connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        // PUT api/SendConnectors/5
        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Put(int id, [FromBody]SendConnector sendConnector)
        {
            var updated = _service.UpdateSendConnector(sendConnector);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update send connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/SendConnectors/5
        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteSendConnector(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete send connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}