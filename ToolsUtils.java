package jp.co.inc.media.vedio.utils;

import javax.imageio.ImageIO;

import javafx.application.Application;
import javafx.scene.Group;
import javafx.scene.Scene;
import javafx.scene.image.Image;
import javafx.scene.image.ImageView;
import javafx.scene.layout.VBox;
import javafx.stage.Stage;

public class ToolsUtils extends Application {

	public static Image screenCapture(int x,int y,int w,int h) {
		 Image image = null;
		 try {
	    	java.awt.Robot robot = new java.awt.Robot();
        	java.awt.image.BufferedImage bi = robot.createScreenCapture(new java.awt.Rectangle(x, y, w, h));
        	java.io.ByteArrayOutputStream stream = new java.io.ByteArrayOutputStream();
			ImageIO.write(bi, "png", stream);
	        image = new Image(new java.io.ByteArrayInputStream(stream.toByteArray()), w, h, true, true);
		} catch (Exception e) {
			e.printStackTrace();
		}
		return image;
	}
	public void start(final Stage primaryStage) {
		Group root = new Group();

		ImageView imageView = new ImageView(screenCapture(0,0,1000,800));
		VBox vBox = new VBox();
		vBox.getChildren().addAll(imageView);
		root.getChildren().add(vBox);

		Scene scene = new Scene(root, 400, 425);
		primaryStage.setTitle("java-buddy.blogspot.com");
		primaryStage.setScene(scene);
		primaryStage.show();
	}
	public static void main(String[] args) {
		launch(args);
	}
}
