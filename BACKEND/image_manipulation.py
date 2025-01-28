import cv2
import numpy as np
import math
from matplotlib import pyplot as plt
from matplotlib import image as im

# Read image as grayscale.
img = cv2.imread('50328.b368a4a5.jpg', cv2.IMREAD_GRAYSCALE)
assert img is not None, "File not found."

# Perform canny edge detection.
edges = cv2.Canny(img,100,200)
cdst = cv2.cvtColor(edges, cv2.COLOR_GRAY2BGR)

# Perform probabilistic hough transform and draw lines into the image.
lines = cv2.HoughLinesP(edges, 1, np.pi / 180, 50, None, 50, 10)
if lines is not None:
    for i in range(0, len(lines)):
        l = lines[i][0]
        cv2.line(cdst, (l[0], l[1]), (l[2], l[3]), (0,0,255), 3, cv2.LINE_AA)

# Show image.
while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break
