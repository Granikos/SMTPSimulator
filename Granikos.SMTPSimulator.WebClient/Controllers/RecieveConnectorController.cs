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
    [RoutePrefix("api/ReceiveConnectors")]
    public class ReceiveConnectorsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("Default")]
        public ReceiveConnector Default()
        {
            return _service.GetDefaultReceiveConnector();
        }

        // GET api/ReceiveConnectors
        [HttpGet]
        [Route("")]
        public IEnumerable<ReceiveConnector> Get()
        {
            return _service.GetReceiveConnectors();
        }

        // GET api/ReceiveConnectors/5
        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            var ReceiveConnector = _service.GetReceiveConnector(id);

            if (ReceiveConnector == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find Receive connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, ReceiveConnector);
        }

        // POST api/ReceiveConnectors
        [HttpPost]
        [Route("")]
        public HttpResponseMessage Post([FromBody]ReceiveConnector ReceiveConnector)
        {
            var added = _service.AddReceiveConnector(ReceiveConnector);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add Receive connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        // PUT api/ReceiveConnectors/5
        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Put(int id, [FromBody]ReceiveConnector ReceiveConnector)
        {
            var updated = _service.UpdateReceiveConnector(ReceiveConnector);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update Receive connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/ReceiveConnectors/5
        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteReceiveConnector(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete Receive connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}