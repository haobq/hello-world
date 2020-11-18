package jp.co.inc.media.vedio.utils;

import javax.imageio.ImageIO;

import javafx.geometry.Bounds;
import javafx.geometry.Point2D;
import javafx.scene.image.Image;
import javafx.scene.image.ImageView;
import javafx.scene.layout.Pane;
import javafx.stage.Window;

public class ToolsUtils {

	public static Image screenCapture(Pane pane) {
		Image image = null;
		try {

			if ( pane.getParent() != null && pane.getParent().getScene()!= null) {
				Window w = pane.getParent().getScene().getWindow();
				System.out.println("w.getX()="+w.getX());
				System.out.println("w.getX()="+w.getY());

				Bounds bounds = pane.getChildren().get(0).getLayoutBounds();
				Point2D coordinates = pane.localToScene(bounds.getMinX(), bounds.getMinY());
				int X = (int) coordinates.getX()+(int)w.getX();
				int Y = (int) coordinates.getY()+(int)w.getY();
				int width = (int) pane.getWidth();
				int height = (int) pane.getHeight();
				System.out.println("X="+X);
				System.out.println("Y="+Y);
				System.out.println("width="+width);
				System.out.println("height="+height);
				java.awt.Rectangle screenRect = new java.awt.Rectangle(X, Y+20, width, height);
				java.awt.Robot robot = new java.awt.Robot();
				java.awt.image.BufferedImage bi = robot.createScreenCapture(screenRect);
				java.io.ByteArrayOutputStream stream = new java.io.ByteArrayOutputStream();
				ImageIO.write(bi, "png", stream);
				image = new Image(new java.io.ByteArrayInputStream(stream.toByteArray()), width, height, true, true);
			}
		} catch (Exception e) {
			e.printStackTrace();
		}
		return image;
	}

	public static void setImageView(Pane pane, Pane subPane) {
		Image image = screenCapture(pane);
		ImageView imageView = new ImageView(image);
		imageView.setFitWidth(subPane.getWidth());
		imageView.setFitHeight(pane.getHeight()/3.33);
		subPane.getChildren().clear();
		subPane.getChildren().add(imageView);
	}
}
