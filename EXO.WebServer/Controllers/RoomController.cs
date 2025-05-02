using EXO.WebServer.Server.Rooms;
using Microsoft.AspNetCore.Mvc;

namespace EXO.WebServer.Controllers
{

    [ApiController]
    [Route("[Controller]")]
    public class RoomController : Controller
    {

        RoomManager roomManager;

        public RoomController(RoomManager _roomManager)
        { 
            roomManager = _roomManager;
        }


        [HttpGet("[Action]")]
        public ActionResult<IEnumerable<RoomManager.RoomRecord>> GetRoomRecords()
        {
            return Ok(roomManager.GetRoomRecords());
        }
    }
}
