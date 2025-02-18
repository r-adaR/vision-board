import cv2
import numpy as np
import math
from matplotlib import pyplot as plt
from matplotlib import image as im
import sklearn.cluster as cluster

# Enumeration
EPSILON = 60

# Read image as grayscale.
img = cv2.imread('easy_0.png', cv2.IMREAD_GRAYSCALE)
assert img is not None, "File not found."

print("begin")

# Perform canny edge detection.
edges = cv2.Canny(img,100,200)
cdst = cv2.cvtColor(edges, cv2.COLOR_GRAY2BGR)

# -------------------------------------------------------- Derek TODO:

# (FUNCTION) Perform probabilistic hough transform and draw lines into the image
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





# (FUNCTION) Agglomerative Clustering: (sort into vertical & horizontal lines)

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

print(slope_clusters)
print(len(slope_clusters))



# (DBSCAN FUNCTION) calculate the mean x and mean y
sum_x = 0
sum_y = 0
num_directional_lines = 0
LINE_DIRECTION = 0

# track max and min x value and/or y value

for i in range(len(lines)):
    # only counts vertical or horizontal lines (depending on set to 0 or 1)
    if slope_clusters[i] == LINE_DIRECTION:
        cv2.line(cdst, (lines[i][0][0], lines[i][0][1]), (lines[i][0][2], lines[i][0][3]), (255,0,0), 3, cv2.LINE_AA)
        sum_x += lines[i][0][0] + lines[i][0][2]
        sum_y += lines[i][0][1] + lines[i][0][3]
        num_directional_lines += 1

avg_x = int(sum_x / (2 * num_directional_lines))
avg_y = int(sum_y / (2 * num_directional_lines))



# show image
"""cv2.circle(cdst, (avg_x, avg_y), 5, (0, 255, 0), 10)
while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break"""



# print("line slopes")
print(line_slopes)

# -------------------------------------------------------- Phillip TODO:

# (DBSCAN FUNCTION) average slope 
avg_angle = 0
for i in range(len(lines)):
    # only counts vertical or horizontal lines (depending on set to 0 or 1)
    if slope_clusters[i] == LINE_DIRECTION:
        avg_angle += math.atan(line_slopes[i])



# avg_slope = avg_slope / (2 * num_directional_lines)
avg_slope = math.tan(avg_angle / (num_directional_lines))

print("AVERAGE SLOPE")
print(avg_slope)


#cv2.line(cdst, (avg_x, avg_y), (avg_x + 100, avg_y + int((avg_slope * 100)) ), (255,0,0), 5, cv2.LINE_AA)

"""while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break"""
        

# find intercept of horizontal lines with mean vertical line
# y = mx + b

# calculate y-intercept (b) of mean vertical line
# b = y - mx
y_intercept_avg_vert_line = avg_y - (avg_slope * avg_x)


# calcualte y-intercept (b) for each horizontal line
intersections = []

for i in range(len(lines)):
    # only counts vertical or horizontal lines (depending on set to 0 or 1)
    if slope_clusters[i] != LINE_DIRECTION:
        # b = y - mx
        y_intercept_horizontal_line = lines[i][0][1] - (line_slopes[i] * lines[i][0][0])
        intersection_x = (y_intercept_avg_vert_line - y_intercept_horizontal_line) / (line_slopes[i] - avg_slope)
        intersection_y = ((avg_slope * intersection_x) + y_intercept_avg_vert_line)
        intersections.append([intersection_x, intersection_y])
        #cv2.circle(cdst, (int(intersection_x), int(intersection_y)), 5, (0, 255, 0), 10)


while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break

# DBSCAN
# (FUNCTION)
intercept_clusters = cluster.DBSCAN(eps=EPSILON, min_samples=1).fit(np.array(intersections))
print(intercept_clusters.labels_)

for i in range(len(intersections)):
    if intercept_clusters.labels_[i] >= 0:
        cv2.circle(cdst, (int(intersections[i][0]), int(intersections[i][1])), 5, (int(intercept_clusters.labels_[i]*21), 0, 0), 10)

        
while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break

# (FUNCTION)
cluster_dict = dict()

for i in range(len(intercept_clusters.labels_)):
    if intercept_clusters.labels_[i] not in cluster_dict:
        cluster_dict[intercept_clusters.labels_[i]] = [intersections[i]]
    else:
        cluster_dict[intercept_clusters.labels_[i]].append(intersections[i])

# (FUNCTION) Calculate the average point and slope for each cluster.
merged_points = dict()
for c in cluster_dict:
    point_sum = [0.0, 0.0]
    
    for p in cluster_dict[c]:
        point_sum[0] += p[0]
        point_sum[1] += p[1]
    
    point_average = [(point_sum[0]/len(cluster_dict[c])), (point_sum[1]/len(cluster_dict[c]))]
    merged_points[c] = point_average


for i in merged_points:
    cv2.circle(cdst, (np.int_(merged_points[i][0]), np.int_(merged_points[i][1])), 5, (255, 244, 244), 5)

while True:
    cv2.imshow('test', cdst)
    if cv2.waitKey(1) == ord('q'):
        break