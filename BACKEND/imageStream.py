import base64
import cv2



# open camera
cap = cv2.VideoCapture(0)
assert cap.isOpened(), "Cannot open camera"
# if not cap.isOpened():
#     print("Error: Could not open camera.")
#     exit()



# read frames rom camera
while True:
    ret, frame = cap.read()
    if not ret:
        print("Error: Can't receive frame (stream end?). Exiting ...")
        break
    cv2.imshow('Camera Feed', frame)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break


# encode the current frame into a png (in working memory)
img_encode = cv2.imencode('.png', frame)[1]

# converts the png into base64
base64_string = base64.b64encode(img_encode)

# saves the text to a file (not needed in final product)
with open('image.txt', 'w') as file:
    file.write(str(base64_string2))

# prints string (not needed in final product)
if base64_string:
    print(base64_string)


# NEXT STEP --> send base64 over network to unity


