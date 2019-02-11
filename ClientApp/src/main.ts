import '@babel/polyfill';
import Vue from 'vue';
import './plugins/axios';
import './plugins/vuetify';
import App from './App.vue';
import router from './router';
import store from '@/store/index';
import './registerServiceWorker';

import axios, { AxiosResponse } from 'axios';

// Add a response interceptor
axios.interceptors.response.use((response: AxiosResponse<any>) => {
    // Do something with response data
    return response;
}, (error: any) => {

    if (error.response.status === 401) {
        window.location = error.response.headers.location;
    }

    // Do something with response error
    return Promise.reject(error);
});

Vue.config.productionTip = false;

new Vue({
  router,
  store,
  render: (h) => h(App),
}).$mount('#app');
