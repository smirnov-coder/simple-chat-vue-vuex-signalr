import { DefaultHttpClient } from "@aspnet/signalr";

//
// К сожалению, так и не удалось найти в SignalR встроенной возможности обработки различных HTTP-ответов, таких как 401.
// Поэтому пришлось модифицировать DefaultHttpClient, предоставляемый SignalR (который, судя по всему, для этих целей и
// предназначен), вызывающий callback-функцию при получении "плохого" ответа.
//
export class CustomHttpClient extends DefaultHttpClient {
    constructor(logger) {
        super(logger);
        this.onErrorResponse = null;
    }

    send(request) {
        return super.send(request)
            .catch(({ statusCode }) => {
                if (this.onErrorResponse) {
                    this.onErrorResponse(statusCode);
                }
                return Promise.reject(error);
            });
    }
}
