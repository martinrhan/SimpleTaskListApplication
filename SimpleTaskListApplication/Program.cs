Dictionary<string, TaskItem> TaskDictionary = new();

Command[] CommandArray = [
    new(){
        Name = "AddTask",
        Execute = (args) => {
            string name = args[0];
            if (!DateTime.TryParse(args[1], out DateTime dueDate)) {
                Console.WriteLine("Invalid date format. Please use YYYY-MM-DD.");
                return;
            }
            if (TaskDictionary.ContainsKey(name)) {
                Console.WriteLine($"Task '{name}' already exists.");
                return;
            }
            TaskDictionary[name] = new TaskItem { Name = name, DueDate = dueDate, IsCompleted = false };
            Console.WriteLine($"Task '{name}' added with due date {dueDate.ToShortDateString()}.");
        },
        UsageInfo = "AddTask <Name> <DueDate>"
    },
    new(){
        Name = "ViewTasks",
        Execute = (args) => {
            if (TaskDictionary.Count == 0) {
                Console.WriteLine("No tasks available.");
                return;
            }
            IOrderedEnumerable<TaskItem> orderedTasks;
            switch (args[0]){
                case "Name":
                    switch (args[1]){
                        case "true":
                            orderedTasks = TaskDictionary.Values.OrderByDescending(t => t.Name);
                            break;
                        case "false":
                            orderedTasks = TaskDictionary.Values.OrderBy(t => t.Name);
                            break;
                        default:
                            Console.WriteLine("Invalid IsDescending value. Use true or false.");
                            return;
                    }
                    break;
                case "DueDate":
                    switch (args[1]){
                        case "true":
                            orderedTasks = TaskDictionary.Values.OrderByDescending(t => t.DueDate);
                            break;
                        case "false":
                            orderedTasks = TaskDictionary.Values.OrderBy(t => t.DueDate);
                            break;
                        default:
                            Console.WriteLine("Invalid IsDescending value. Use true or false.");
                            return;
                    }
                    break;
                case "Status":
                    switch (args[1]){
                        case "true":
                            orderedTasks = TaskDictionary.Values.OrderByDescending(t => t.IsCompleted);
                            break;
                        case "false":
                            orderedTasks = TaskDictionary.Values.OrderBy(t => t.IsCompleted);
                            break;
                        default:
                            Console.WriteLine("Invalid IsDescending value. Use true or false.");
                            return;
                    }
                    break;
                default:
                    Console.WriteLine("Invalid OrderBy value. Use Name or DueDate.");
                    return;
            }
            TaskDictionary.Values.OrderBy(t => t.Name);
            int longestNameLength = TaskDictionary.Values.Max(t => t.Name.Length);
            string format = "{0,-10} {1,-15} {2,-10}";
            Console.WriteLine(format,"Name", "DueDate", "Status");
            foreach (var task in orderedTasks) {
                string status = task.IsCompleted ? "Completed" : "Pending";
                Console.ForegroundColor = task.IsCompleted ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(format, task.Name, task.DueDate.ToShortDateString(), status);
            }
            Console.ResetColor();
        },
        UsageInfo = "ViewTasks <OrderBy> <IsDescending>"
    },
    new(){
        Name = "MarkTask",
        Execute = (args) => {
            if (args.Length < 2) {
                Console.WriteLine("Usage: MarkTask <TaskName> <IsCompleted>");
                return;
            }
            string taskName = args[0];
            if (!TaskDictionary.ContainsKey(taskName)) {
                Console.WriteLine($"Task '{taskName}' does not exist.");
                return;
            }
            if (!bool.TryParse(args[1], out bool isCompleted)) {
                Console.WriteLine("Invalid value for IsCompleted. Use true or false.");
                return;
            }
            TaskDictionary[taskName].IsCompleted = isCompleted;
            Console.WriteLine($"Task '{taskName}' marked as {(isCompleted ? "completed" : "pending")}.");
        },
        UsageInfo = "MarkTask <TaskName> <IsCompleted>"
    },
    new(){
        Name = "DeleteTask",
        Execute = (args) => {
            if (args.Length < 1) {
                Console.WriteLine("Usage: DeleteTask <TaskName>");
                return;
            }
            string taskName = args[0];
            if (!TaskDictionary.ContainsKey(taskName)) {
                Console.WriteLine($"Task '{taskName}' does not exist.");
                return;
            }
            TaskDictionary.Remove(taskName);
            Console.WriteLine($"Task '{taskName}' deleted.");
        },
        UsageInfo = "DeleteTask <TaskName>"
    },
    new (){
        Name = "SetTaskSortOrder",
        Execute = (args) => {
            Console.WriteLine("SetTaskSortOrder command is not implemented yet.");
        },
    },
    new (){
        Name = "Help"
    }
    ];
Dictionary<string, Command> CommandDictionary = CommandArray.ToDictionary(c => c.Name, c => c);
CommandDictionary["Help"].Execute = (args) => ShowHelp();

void ShowHelp() {
    Console.WriteLine("Available commands:");
    foreach (var command in CommandArray) {
        Console.WriteLine($"- {command.UsageInfo}");
    }
}

Console.WriteLine("Simple Task List Application v1.0");
Console.WriteLine("Following is available commands. Use Help command to show this again.");
ShowHelp();
Console.WriteLine();
while (true) {
    string? input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) {
        continue;
    }
    string[] inputs = SeperateInputString(input);
    Command? command;
    if (CommandDictionary.TryGetValue(inputs[0], out command)) {
        if (inputs.Length - 1 < command.ArgumentCount) {
            Console.WriteLine($"Insufficient arguments. Usage: {command.UsageInfo}");
            Console.WriteLine();
            continue;
        }
        command.Execute!(inputs[1..]);
    } else {
        Console.WriteLine("Invalid command.");
    }
    Console.WriteLine();
}

string[] SeperateInputString(string input) {
    List<string> result = new();

    int i = 0;
    int FindNext(char c) {
        for (int i_local = i + 1; i_local < input.Length; i_local++) {
            if (input[i_local] == c) {
                return i_local;
            }
        }
        return -1;
    }

    while (i < input.Length) {
        if (input[i] == '"') {
            int nextQuote = FindNext('"');
            if (nextQuote == -1) {
                throw new Exception("Mismatched quotes in input.");
            } else {
                result.Add(input[(i + 1)..nextQuote]);
                i = nextQuote + 1;
            }
        } else {
            int nextSpace = FindNext(' ');
            if (nextSpace == -1) {
                result.Add(input[i..].Trim());
                break;
            } else {
                result.Add(input[i..nextSpace].Trim());
                i = nextSpace + 1;
            }
        }
    }
    return result.ToArray();
}


public class Command {
    public required string Name { get; init; }

    public Action<string[]>? Execute { get; set; }

    public int ArgumentCount => UsageInfo.Split(' ').Length - 1;

    public string UsageInfo {
        get => field ??= Name;
        init;
    }
}


class TaskItem {
    public string Name { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
}
