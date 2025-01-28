import cv2
import numpy as np
import math
from matplotlib import pyplot as plt
from matplotlib import image as im
import sklearn.cluster as cluster

# Read image as grayscale.
img = cv2.imread('vision-board/BACKEND/checkboard.jpg', cv2.IMREAD_GRAYSCALE)
assert img is not None, "File not found."

# Perform canny edge detection.
edges = cv2.Canny(img,100,200)
cdst = cv2.cvtColor(edges, cv2.COLOR_GRAY2BGR)

# Perform probabilistic hough transform and draw lines into the image.
lines = cv2.HoughLinesP(edges, 1, np.pi / 180, 50, None, 50, 10)
print(lines)
if lines is not None:
    for i in range(0, len(lines)):
        l = lines[i][0]
        cv2.line(cdst, (l[0], l[1]), (l[2], l[3]), (0,0,255), 3, cv2.LINE_AA)

# Show image.
while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break



# Agglomerative Clustering: (sort into vertical & horizontal lines)

# make model parameters
agglom_model = cluster.AgglomerativeClustering(n_clusters=2, metric='euclidean', memory=None, connectivity=None, 
                                        compute_full_tree='auto', linkage='single', 
                                        distance_threshold=None, compute_distances=False)


# convert line data (two x,y point values) into a slope value
line_slopes = []
for line in lines:
    line_slope = 0
    if line[0][0] - line[0][2] == 0:
        line_slope = 99999
    else:
        line_slope = (line[0][1] - line[0][3]) / (line[0][0] - line[0][2])  
    line_slopes.append(line_slope)



# fit and predict the lines --> returns each line's cluster (horizontal or vertical)
# NOTE: just predicting the lines on the training data
slopes = np.array(line_slopes).reshape(-1, 1)
slope_clusters = agglom_model.fit_predict(slopes)

