using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using CSharpFunctionalExtensions;

public class EditRoomNameService : IEditRoomNameServices
{
    private readonly string NameRegex = $@"^{0,100}$";
    private readonly DbContextModel _context;

    public EditRoomNameService(DbContextModel context)
    {
        _context = context;
    }

    public async Task<Result<string>> ChangeName(Guid roomId, string name)
    {
        if (!Regex.IsMatch(name, NameRegex) || string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<string>("Error: Invalid character.");
        }
        if (!CheckValidName(name))
        {
            return Result.Failure<string>("Error: Invalid name. please, choose other name.");
        }

        try
        {
            var room = await _context.Room.FindAsync(roomId);
            _context.Room.Attach(room);
            room.Name = name;
            await _context.SaveChangesAsync();

            return Result.Success<string>("Success: Changed name.");
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<string>("An unexpected server error ocurred");
        }
    }

    private bool CheckValidName(string name)
    {
        // I did this way cause it's simpler
        var exist = _context.Room.Any(r => r.Name == name);
        if (!exist)
        {
            return true;
        }

        return false;
    }
}
