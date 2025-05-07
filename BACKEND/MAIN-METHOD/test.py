from VisionBoard import vision_board_reader
import cv2 as cv

if __name__ == '__main__':
    for i in range(10, 19):
        print(f"easy_{i}.png")
        test_image = cv.imread(f"training_data/Easy/easy_{i}.png")
        board = vision_board_reader(test_image)
        for i in range(len(board)):
            for j in range(len(board[i])):
                if board[i][j] == '':
                    board[i][j] = ' '
        print(board)