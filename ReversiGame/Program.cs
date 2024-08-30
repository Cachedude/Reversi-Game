using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

char?[,] GameBoard;


string GetPlayerMove(char playerTile){
    List<int> validInputs = new List<int>{ 0, 1, 2, 3, 4, 5, 6, 7};
    int inputRow, inputCol;

    while(true) {
        Console.Write("Enter Move (Enter 'quit' to Quit, 'hints' for turning On/Off Hints):");
        string value = Console.ReadLine().ToLower();
        if(value == "quit") return "quit";
        if(value == "hints") return "hints";

        if(value.Length == 2 && int.TryParse(value, out int move))
        {
            inputRow = Convert.ToInt32(value[0].ToString()) - 1;
            inputCol = Convert.ToInt32(value[1].ToString()) - 1;
            if(validInputs.Contains(inputRow) && validInputs.Contains(inputCol)){
                if (IsValidMove(playerTile, inputRow, inputCol).Count == 0)
                {
                    Console.WriteLine("Invalid Move. Please type the row digit(1-8), then the y digit (1-8).");
                    continue;
                }
                else
                    break;
            }
            else{
                Console.WriteLine("Invalid Move. Please type the row digit(1-8), then the y digit (1-8).");
                Console.WriteLine("For example, 81 will be the top-right corner");
            }
        }
    }
    return $"{inputRow}{inputCol}";
}

(int, int) GetComputerMove(char computerTile){
    List<(int, int)> possibleMoves = GetValidMoves(computerTile);

    possibleMoves = GenerateRandomLoop(possibleMoves);

    return (possibleMoves[0].Item1, possibleMoves[0].Item2);
}

bool MakeMove(char tile, int rowStart, int colStart){
    List<(int, int)> tilesToFlip = IsValidMove(tile, rowStart, colStart);

    if(tilesToFlip.Count == 0){
        return false;
    }

    GameBoard[rowStart, colStart] = tile;
    foreach(var (row, col) in tilesToFlip){
        GameBoard[row, col] = tile;
    }
    return true;
}

string WhoGoesFirst(){
    if(new Random().Next(1) == 0)
        return "Computer";
    else
        return "Player";
}

(char, char) EnterPlayerTile(){
    char enteredTile;
    do
    {
        enteredTile = Console.ReadKey().KeyChar;
    } while(enteredTile != 'X' && enteredTile != 'O' && enteredTile != 'x' && enteredTile != 'o');
    if(enteredTile == 'X' || enteredTile == 'x')
        return ('X', 'O');
    else
        return ('O', 'X');
}

List<(int, int)> IsValidMove(char tile, int rowStart, int colStart){
    List<(int, int)> tilesToFlip = new List<(int, int)>();

    if(HasValue(GameBoard, rowStart, colStart) || !IsEmptyValidCell(rowStart, colStart))
        return tilesToFlip;

    GameBoard[rowStart, colStart] = tile;

    char otherTile = tile == 'X' ? 'O': 'X';

    List<(int, int)> directions = new List<(int, int)>{(0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1)};
    foreach(var (rowDirect, colDirect) in directions)
    {
        int row = rowStart, col = colStart;
        row += rowDirect;
        col += colDirect;
        if(IsEmptyValidCell(row, col) && GameBoard[row,col]== otherTile)
        {
            row += rowDirect;
            col += colDirect;
            if(!IsEmptyValidCell(row, col))
                continue;
            while(GameBoard[row, col] == otherTile){
                row += rowDirect;
                col += colDirect;
                if(!IsEmptyValidCell(row, col))
                    break;
            }
            if(!IsEmptyValidCell(row,col))
                continue;
            if(GameBoard[row, col] == tile){
                do{
                    row -= rowDirect;
                    col -= colDirect;
                    if(row == rowStart && col == colStart)
                        break;
                    tilesToFlip.Add((row, col));
                } while(row != rowStart || col != colStart);
            }
        }
    }
    GameBoard[rowStart, colStart] = null;
    return tilesToFlip;
}

List<(int, int)> GetValidMoves(char tile){
    List<(int, int)> validMoves = new List<(int, int)>();

    for(int row = 0; row < 8; row++){
        for(int col = 0; col < 8; col++){
            var move = IsValidMove(tile, row, col);
            if(move.Count > 0){
                validMoves.Add((row, col));
            }
        }
    }
    return validMoves;
}

(int, int) GetGameScore(){
    int xScore = 0, oScore = 0;
    for(int row = 0; row < 8; row++){
        for(int col = 0; col < 8; col++){
            xScore += GameBoard[row, col] == 'X' ? 1 : 0;
            oScore += GameBoard[row, col] == 'O' ? 1 : 0;
        }
    }
    Console.WriteLine($"X: {xScore}, O: {oScore}");
    return (xScore, oScore);
}

#region Draw Game Board
void DrawBoardWithValidMoves(char tile){
    char?[,] boardWithValidMoves = (char?[,])GameBoard.Clone();

    foreach(var (row, col) in GetValidMoves(tile)){
        boardWithValidMoves[row, col] = '~';
    }
    
    DrawBoard(boardWithValidMoves);
}

void DrawBoard(char?[,] boardToPrint) {
    string topLine = "  +---+---+---+---+---+---+---+---+";
    string columnLine = "  |   |   |   |   |   |   |   |   |";

    Console.WriteLine("     1   2   3   4   5   6   7   8");
    Console.WriteLine(topLine);

    for(int i = 0; i < 8; i++){
        Console.WriteLine(columnLine);
        Console.Write(i + 1 + " ");
        for(int j = 0; j < 8; j++){
            Console.Write("| " + (HasValue(boardToPrint, i, j) ? boardToPrint[i, j] : ' ') + " ");
        }
        Console.WriteLine("|");
        Console.WriteLine(columnLine);
        Console.WriteLine(topLine);
    }
}
#endregion

void ResetBoard(){
    GameBoard = new char?[8, 8];

    GameBoard[3,3] = 'X';
    GameBoard[3,4] = 'O';
    GameBoard[4,3] = 'O';
    GameBoard[4,4] = 'X';
}

bool HasValue(char?[,] gameBoard, int row, int col)
{
    bool value = gameBoard[row, col] == 'X' || gameBoard[row, col] == 'O' || gameBoard[row, col] == '~';
    return value;
}

bool IsEmptyValidCell(int row, int col)
{
    if(row < 0 || row > 7 || col < 0 || col > 7){
        return false;
    }
    else
        return true;
}

List<(int, int)> GenerateRandomLoop(List<(int, int)> listToShuffle)
{
    Random _rand = new Random();

    for (int i = listToShuffle.Count - 1; i > 0; i--)
    {
        var k = _rand.Next(i + 1);
        var value = listToShuffle[k];
        listToShuffle[k] = listToShuffle[i];
        listToShuffle[i] = value;
    }
    return listToShuffle;
}

Console.WriteLine("Welcome to Reversi?");
Console.WriteLine();

while (true){
    ResetBoard();

    Console.Write("Please Enter Player Tile (X, O): ");
    var (playerTile, computerTile) = EnterPlayerTile();
    Console.WriteLine();
    bool showHints = false;
    string turn = WhoGoesFirst();
    Console.WriteLine($"The {turn} will go first.");

    while(true){
        if(turn == "Player"){
            if(showHints){
                DrawBoardWithValidMoves(playerTile);
            }
            else{
                DrawBoard(GameBoard);
            }
            GetGameScore();
            var move = GetPlayerMove(playerTile);
            if(move == "quit"){
                Console.WriteLine("Thanks for playing!!");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if(move == "hints"){
                showHints = !showHints;
                continue;
            }
            else{
                MakeMove(playerTile, Convert.ToInt32(move[0].ToString()), Convert.ToInt32(move[1].ToString()));
            }

            if(GetValidMoves(computerTile).Count == 0){
                break;
            }
            else{
                turn = "Computer";
            }
        }
        else{
            DrawBoard(GameBoard);
            GetGameScore();
            Console.Write("Press Enter to see the computer\'s move.");
            Console.ReadLine();
            var (row, col) = GetComputerMove(computerTile);
            MakeMove(computerTile, row, col);
            Console.WriteLine($"Computer played ({row + 1}, {col + 1}).");

            if (GetValidMoves(playerTile).Count == 0){
                break;
            }
            else{
                turn = "Player";
            }
        }
    }

    DrawBoard(GameBoard);
    var (xScore, oScore) = GetGameScore();
    string xUser, oUser;
    if (playerTile == 'X')
    {
        xUser = "Player";
        oUser = "Computer";
    }
    else
    {
        xUser = "Computer";
        oUser = "Player";
    }
    if (xScore > oScore)
    {
        Console.WriteLine($"{xUser} Wins {xScore} - {oScore}");
    }
    else
    {
        Console.WriteLine($"{oUser} Wins {oScore} - {xScore}");
    }
    Console.Write("Press Enter to play again.");
    Console.ReadLine();
    Console.WriteLine();

}