import cv2
import numpy as np

# Param: 
def hough(edges, cdst):
    # (FUNCTION) Perform probabilistic hough transform and draw lines into the image
    lines = cv2.HoughLinesP(edges, 1, np.pi / 180, 50, None, 50, 10)
    print(lines)
    if lines is not None:
        for i in range(0, len(lines)):
            l = lines[i][0]
            cv2.line(cdst, (l[0], l[1]), (l[2], l[3]), (0,0,255), 3, cv2.LINE_AA)
    return lines