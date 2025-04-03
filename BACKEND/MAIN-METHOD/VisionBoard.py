from BoardLocator import board_to_overhead
from PieceClassifier import read_board

class BoardReadError(Exception):
    pass

def vision_board_reader(input_image):
    readjusted_image = board_to_overhead(input_image)
    if readjusted_image is None:
        raise BoardReadError("Failed to detect board! Could not locate board outline!")
        
    final_board = read_board(readjusted_image)
    if final_board is None:
        raise BoardReadError("Failed to read board! Unknown tile read!")
        
    return final_board