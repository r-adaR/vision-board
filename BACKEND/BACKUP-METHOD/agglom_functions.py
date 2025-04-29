import sklearn.cluster as cluster
import numpy as np


def calculate_slopes(lines):
    # convert line data (two x,y point values) into a slope value
    line_slopes = []
    for line in lines:
        line_slope = 0
        if line[0][0] - line[0][2] == 0:
            line_slope = 99999
        else:
            line_slope = (line[0][1] - line[0][3]) / (line[0][0] - line[0][2])  
        line_slopes.append(line_slope)
    return line_slopes


def agglom_cluster(lines, line_slopes):
    agglom_model = cluster.AgglomerativeClustering(n_clusters=2, metric='euclidean', memory=None, connectivity=None, 
                                            compute_full_tree='auto', linkage='single', 
                                            distance_threshold=None, compute_distances=False)

    # fit and predict the lines --> returns each line's cluster (horizontal or vertical)
    # NOTE: just predicting the lines on the training data
    slopes = np.array(line_slopes).reshape(-1, 1)
    slope_clusters = agglom_model.fit_predict(slopes)

    return slope_clusters
