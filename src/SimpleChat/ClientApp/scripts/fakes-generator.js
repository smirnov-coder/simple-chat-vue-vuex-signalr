import faker from "faker";
import { Message } from "@/store/modules/messages";
import { Provider } from "@/scripts/provider-helper";

// Вспомогательный класс для генерации тестовых данных.
export default class FakesGenerator {
    static _fakeUsers = [];
    static _fakeMessages = [];

    constructor() {
        let
            fakeUsers = FakesGenerator._fakeUsers,
            fakeMessages = FakesGenerator._fakeMessages;

        if (fakeUsers.length === 0) {
            for (let index = 0; index < 30; index++) {
                fakeUsers.push({
                    id: index.toString(),
                    name: faker.name.findName(),
                    avatar: faker.image.avatar(),
                    provider: faker.random.arrayElement([Provider.FACEBOOK, Provider.VKONTAKTE,
                        Provider.ODNOKLASSNIKI]),
                    connectionIds: []
                });
            }
        }

        if (fakeMessages.length === 0) {
            for (var index = 0; index < 30; index++) {
                fakeMessages.push(new Message(
                    faker.random.arrayElement(fakeUsers),
                    faker.lorem.text()
                ));
            }
        }
    }

    getFakeUsers() {
        return FakesGenerator._fakeUsers;
    }

    getFakeMessages() {
        return FakesGenerator._fakeMessages;
    }
}
