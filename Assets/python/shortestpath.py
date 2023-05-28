def DFS(start, goal):
    stack = []
    stack.append(start)

    visited = set()
    parent = {}

    foundPath = False  # Flag to track if a path has been found

    while stack:
        current = stack.pop()

        if current == goal:
            foundPath = True
            # print("YAY KETEMU")
            break  # Exit the search if the goal is reached

        visited.add(current)

        # Add the neighboring cells to the stack
        neighbors = get_neighbors(current)
        for neighbor in neighbors:
            if neighbor not in visited:
                stack.append(neighbor)
                parent[neighbor] = current

    if foundPath and goal not in parent:
        parent[goal] = goal

    # Highlight the path from the power-up to the goal if found
    if foundPath and goal in parent:
        dfsStack = []  # Create a new stack to store the path
        node = goal
        while node != start:
            dfsStack.append(node)  # Push the cells on the path to the stack
            node = parent[node]
        dfsStack.append(start)  # Push the start position to the stack

    return dfsStack if foundPath and goal in parent else None


def get_neighbors(cell):
    x, y = cell
    neighbors = [(x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)]
    return neighbors


# Contoh penggunaan fungsi DFS

# Contoh grid 5x5
grid = [[0, 0, 0, 0, 0],
        [0, 1, 1, 0, 0],
        [0, 0, 1, 0, 0],
        [0, 1, 1, 1, 0],
        [0, 0, 0, 0, 0]]

start = (0, 0)
goal = (4, 4)

path = DFS(start, goal)
if path:
    print("Path found:")
    for cell in path:
        print(cell)
else:
    print("No path found.")