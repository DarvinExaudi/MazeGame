def DFS(start, goal):
    stack = [(start, [start])]  # stack untuk menyimpan node yang akan dieksplorasi
    visited = set()  # set untuk melacak node yang sudah dikunjungi

    while stack:
        node, path = stack.pop()  # ambil node dari stack

        if node == goal:
            return path  # mengembalikan jalur jika node yang dicari ditemukan

        visited.add(node)  # tandai node sebagai sudah dikunjungi

        # Dapatkan tetangga-tetangga dari node saat ini
        neighbors = GetNeighbors(node)

        for neighbor in neighbors:
            if neighbor not in visited:
                stack.append((neighbor, path + [neighbor]))  # tambahkan tetangga ke stack dengan jalur yang diperbarui

    return None  # mengembalikan None jika tidak ada jalur yang ditemukan


def GetNeighbors(position):
    # Fungsi ini mengembalikan tetangga-tetangga dari suatu posisi
    # Dalam contoh ini, tetangga-tetangga dianggap sebagai posisi-posisi yang bersebelahan secara horizontal atau vertikal

    x, y = position
    neighbors = [(x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)]  # tetangga atas, bawah, kiri, dan kanan

    # Hapus tetangga-tetangga yang berada di luar batasan ruang pencarian

    neighbors = [(nx, ny) for nx, ny in neighbors if
                 0 <= nx < 5 and 0 <= ny < 5]  # asumsi ruang pencarian adalah matriks 5x5

    return neighbors


# Contoh penggunaan fungsi DFS

start_node = (0, 0)
goal_node = (4, 4)

path = DFS(start_node, goal_node)

if path:
    print("Jalur yang ditemukan:", path)
else:
    print("Tidak ada jalur yang ditemukan.")