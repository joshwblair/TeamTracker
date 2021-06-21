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
    public class TeamsController : ControllerBase
    {
        private readonly ILogger<TeamsController> _logger;
        private readonly TeamTrackerContext _dbContext;

        public TeamsController(ILogger<TeamsController> logger, TeamTrackerContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<Team> GetTeamById(int id) =>
            await _dbContext.Teams.Where(t => t.Id == id).Include(t => t.Players).FirstOrDefaultAsync();

        [HttpGet]
        public async Task<IEnumerable<Team>> GetTeams(string sortBy)
        {
            switch (sortBy?.ToLower())
            {
                case "name" :
                    return await _dbContext.Teams.OrderBy(t => t.Name).Include(t => t.Players).ToListAsync();
                case "location" :
                    return await _dbContext.Teams.OrderBy(t => t.Location).Include(t => t.Players).ToListAsync();
                default :
                    return await _dbContext.Teams.OrderBy(t => t.Id).Include(t => t.Players).ToListAsync();

            }
        }

        
        [HttpPut]
        [Route("{teamId}/Players/{playerId}")]
        public async Task<ActionResult<Team>> AddPlayer(int teamId, int playerId)
        {
            var playerToAdd = await _dbContext.Players.FindAsync(playerId);
            if(playerToAdd == null)
            {
                return BadRequest($"No player exists with id: {playerId}");
            }
            else if (playerToAdd.IsOnTeam)
            {
                return BadRequest($"Player is already on a team");
            }

            var team = await _dbContext.Teams.Where(t => t.Id == teamId).Include(t => t.Players).FirstOrDefaultAsync();
            if(team == null)
            {
                return BadRequest($"No team exists with id: {teamId}");
            }
            else if (team.Players.Count() >= 8)
            {
                return BadRequest($"Team already contains 8 players");
            }

            playerToAdd.IsOnTeam = true;
            team.Players.Add(playerToAdd);
            _dbContext.Teams.Update(team);
            await _dbContext.SaveChangesAsync();

            return Ok(team);
        }

        [HttpDelete]
        [Route("{teamId}/Players/{playerId}")]
        public async Task<ActionResult<Team>> RemovePlayer(int teamId, int playerId)
        {
            var team = await _dbContext.Teams.Where(t => t.Id == teamId).Include(t => t.Players).FirstOrDefaultAsync();
            if(team == null)
            {
                return BadRequest($"No team exists with id: {teamId}");
            }

            var playerToRemove = team.Players.Where(p => p.Id == playerId).FirstOrDefault();
            if(playerToRemove == null)
            {
                return BadRequest($"No player with id: {playerId} exists on team: {team.Name}, {team.Location}");
            }

            playerToRemove.IsOnTeam = false;
            team.Players.Remove(playerToRemove);
            _dbContext.Teams.Update(team);
            await _dbContext.SaveChangesAsync();

            return Ok(team);
        }

        [HttpPost]
        public async Task<ActionResult<Team>> CreateTeam(Team newTeam)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teamExists = await (from t in _dbContext.Teams
                                where t.Name.ToLower() == newTeam.Name.ToLower() 
                                && t.Location.ToLower() == newTeam.Location.ToLower()
                                select t ).AnyAsync();

            if(!teamExists)
            {
                newTeam.Id = 0;//set id to 0 so a new one gets generated
                newTeam.Players = new List<Player>();//don't allow new team creation to also create players

                await _dbContext.Teams.AddAsync(newTeam);
                _dbContext.SaveChanges();
                return CreatedAtAction(nameof(GetTeamById), new {id = newTeam.Id}, newTeam);
            }
            else
            {
                return BadRequest("A team with that name and location already exists");    
            }
        }
    }
}
