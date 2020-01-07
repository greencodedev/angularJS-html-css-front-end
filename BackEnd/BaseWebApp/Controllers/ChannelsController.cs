using BaseWebApp.Maven.Channels;
using BaseWebApp.Maven.Users;
using BaseWebApp.Maven.Utils.Api;
using MWSUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BaseWebApp.Controllers
{
    [RoutePrefix("api/channels")]
    public class ChannelsController : BaseApiController
    {
        [HttpGet]
        public HttpResponseMessage getChannels()
        {
           return ReturnResponse(ChannelsProvider.GetChannelOrChannels(UsersProvider.GetCurrentAccountId(), null));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage getChannel(int id)
        {
            return ReturnResponse(ChannelsProvider.GetChannelOrChannels(UsersProvider.GetCurrentAccountId(), id));
        }

        [HttpPut]
        public HttpResponseMessage updateChannel(Channel channel)
        {
           return ReturnResponse(ChannelsProvider.UpdateChannel(channel, UsersProvider.GetCurrentAccountId()));
        }

        [HttpPost]
        public HttpResponseMessage insertChannel(Channel channel)
        {
            return ReturnResponse(ChannelsProvider.InsertChannel(channel, UsersProvider.GetCurrentAccountId()));

        }
       
    }
}
