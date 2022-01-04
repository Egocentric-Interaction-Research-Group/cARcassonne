# How to train a model with Roboflows YOLOv5 notebook and your own dataset

The provided Colab notebook in these instructions was originally created by Roboflow[^1] and edited by Anni Johansson. It uses the YOLOv5x training algorithm.



1. Create a dataset according to the YOLOv5 format.  \
Create a folder containing:
    1. The yaml file named data.yaml according to image 1 below
    2. A folder named “train” including:
        1. A folder named “images” including about 70% of your images 
        2. A folder named “labels” including annotation files (.txt format) for all of these images following the YOLO format [^2], named as its corresponding image


    3. A folder named “test” including:
        3. A folder named “images” including about 20% of your images 
        4. A folder named “labels” including annotation files (.txt format) for all of these images following the YOLO format, named as its corresponding image
    4. A folder named “valid” including:
        5. A folder named “images” including about 10% of your images 
        6. A folder named “labels” including annotation files (.txt format) for all of these images following the YOLO format, named as its corresponding image
    5. Make sure that all images include bounding boxes! Empty annotation files (corresponding to images without bounding boxes) will be a problem when being uploaded to the Colab notebook
2. Open the following Colab notebook and make a copy of it: \
[https://colab.research.google.com/drive/1oKnxaYo5ppRMFurHB-kdzS7T1k43EqBv?usp=sharing](https://colab.research.google.com/drive/1oKnxaYo5ppRMFurHB-kdzS7T1k43EqBv?usp=sharing)
3. Run the first five code cells
4. Add your own train, test and valid folders into the folder of files in the Colab notebook (according to image number 2 below)
5. Add the data.yaml file inside the yolov5 folder (according to image number 3 below)
6. Run the following code cells
7. Use the last two cells in order to export the trained weights to you Google Drive


## Images



<p id="gdcalert1" ><span style="color: red; font-weight: bold">>>>>>  gd2md-html alert: inline image link here (to images/image1.png). Store image on your image server and adjust path/filename/extension if necessary. </span><br>(<a href="#">Back to top</a>)(<a href="#gdcalert2">Next alert</a>)<br><span style="color: red; font-weight: bold">>>>>> </span></p>


![alt_text](images/image1.png "image_tooltip")


Image 1



<p id="gdcalert2" ><span style="color: red; font-weight: bold">>>>>>  gd2md-html alert: inline image link here (to images/image2.png). Store image on your image server and adjust path/filename/extension if necessary. </span><br>(<a href="#">Back to top</a>)(<a href="#gdcalert3">Next alert</a>)<br><span style="color: red; font-weight: bold">>>>>> </span></p>


![alt_text](images/image2.png "image_tooltip")


Image 2



<p id="gdcalert3" ><span style="color: red; font-weight: bold">>>>>>  gd2md-html alert: inline image link here (to images/image3.png). Store image on your image server and adjust path/filename/extension if necessary. </span><br>(<a href="#">Back to top</a>)(<a href="#gdcalert4">Next alert</a>)<br><span style="color: red; font-weight: bold">>>>>> </span></p>


![alt_text](images/image3.png "image_tooltip")


Image 3


<!-- Footnotes themselves at the bottom. -->
## Notes

[^1]:
     [https://roboflow.com/](https://roboflow.com/) (visited 6th of December 2021)

[^2]:

     [https://blog.paperspace.com/train-yolov5-custom-data/](https://blog.paperspace.com/train-yolov5-custom-data/) (visited 6th of December 2021)
