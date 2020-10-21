package jp.co.inc.meida.video.frame;

import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;

import org.apache.http.HttpStatus;
import org.apache.http.NameValuePair;
import org.apache.http.client.ClientProtocolException;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.CloseableHttpResponse;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.message.BasicNameValuePair;
import org.apache.http.util.EntityUtils;

/**
 * HttpClientsのサンプル
 */
class Sample {

	void runSample() {
		Charset charset = StandardCharsets.UTF_8;

		CloseableHttpClient httpclient = HttpClients.createDefault();

		HttpPost request = new HttpPost("http://httpbin.org/post");
		List<NameValuePair> requestParams = new ArrayList<>();
		requestParams.add(new BasicNameValuePair("userid","taro"));
		requestParams.add(new BasicNameValuePair("password","123"));

		System.out.println("requestの実行　「" + request.getRequestLine() + "」");

		CloseableHttpResponse response = null;

		try {
			request.setEntity(new UrlEncodedFormEntity(requestParams));
			response = httpclient.execute(request);

			int status = response.getStatusLine().getStatusCode();
			System.out.println("HTTPステータス:" + status);

			//HTTPステータス:200
			if (status == HttpStatus.SC_OK){
				String responseData =	EntityUtils.toString(response.getEntity(),charset);
				System.out.println(responseData);
				//取得したデータが表示される
			}
		} catch (ClientProtocolException e) {
			e.printStackTrace();
		} catch (UnsupportedEncodingException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		} finally {
			try {
				if (response != null) {
					response.close();
				}
				if (httpclient != null) {
					httpclient.close();
				}
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	}
}
