using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TeamTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace TeamTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly ILogger<PlayersController> _logger;
        private readonly TeamTrackerContext _dbContext;

        public PlayersController(ILogger<PlayersController> logger, TeamTrackerContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IEnumerable<Player>> GetPlayers(string lastName)
        {
            if(!string.IsNullOrEmpty(lastName))
            {
                return await _dbContext.Players
                        .Where(p => p.LastName.ToLower() == lastName.ToLower())
                        .OrderBy(p => p.Id)
                        .ToListAsync();
            }

            return await _dbContext.Players
                    .OrderBy(t => t.Id)
                    .ToListAsync();
        }


        [HttpGet("{id}")]
        public async Task<Player> GetPlayerById(int id) =>
            await _dbContext.Players.FindAsync(id);
        

        [HttpPost]
        public async Task<ActionResult<Player>> CreatePlayer(Player newPlayer)
        {
            //validate model
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            newPlayer.Id = 0;//set id to 0 so one gets generated
            newPlayer.IsOnTeam = false;//player needs to be added to team through team endpoint

            await _dbContext.Players.AddAsync(newPlayer);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetPlayerById), new {id = newPlayer.Id}, newPlayer);
            
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Player>> DeletePlayer(int id)
        {
            //validate player exists
            var playerToRemove = await GetPlayerById(id);
            if(playerToRemove == null)
            {
                return BadRequest($"Player with Id: {id} does not exist");
            }

            //remove player from any teams they're on
            var playersTeam = await _dbContext.Teams
                                .Include(t => t.Players)
                                .FirstOrDefaultAsync(
                                    t => t.Players.Where(p => p.Id == id).Any()
                                );

            if(playersTeam != null)
            {
                playersTeam.Players.Remove(playerToRemove);
                _dbContext.Teams.Update(playersTeam);
            }

            //Now remove player
            _dbContext.Players.Remove(playerToRemove);

            //save changes and return 202 with removed player
            await _dbContext.SaveChangesAsync();
            return Accepted(playerToRemove);
        }
    }
}
