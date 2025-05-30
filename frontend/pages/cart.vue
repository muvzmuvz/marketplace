<script setup>
import { ref, computed, onMounted } from 'vue';
import '@/assets/css/cart.css';
import { ShoppingCartIcon, CheckIcon } from 'lucide-vue-next'

const isOrderPlaced = ref(false)




// Список товаров в корзине
const cartItems = ref([]);
const userId = ref(null); // Для хранения userId


// Функция для загрузки данных о пользователе
const fetchUser = async () => {
  try {
    const response = await fetch('http://localhost:8080/user/user', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include', // Если нужно отправлять куки
    });

    if (response.ok) {
      const data = await response.json();
      userId.value = data.id; // Записываем userId в переменную
      console.log('Данные о пользователе:', data);
    } else {
      throw new Error('Ошибка при загрузке данных о пользователе');
    }
  } catch (error) {
    console.error(error);
  }
};

// Функция для загрузки корзины
const fetchCartItems = async () => {
  try {
    const response = await fetch('http://localhost:8080/cart/products', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include', // Если нужно отправлять куки
    });

    if (response.ok) {
      const data = await response.json();

      // Преобразуем структуру данных
      cartItems.value = data.map(item => ({
        id: item.product.id,
        name: item.product.name,
        price: item.product.price,
        description: item.product.description,
        imagePath: item.product.images[0].path,
        quantity: item.quantity ?? 1, // Если quantity не передан, ставим 1
      }));

      console.log('Загруженные товары:', cartItems.value);
    } else {
      throw new Error('Ошибка при загрузке товаров из корзины');
    }
  } catch (error) {
    console.error(error);
  }
};

// Функция для удаления товара из корзины
const removeItem = async (productId) => {
  try {
    const response = await fetch(`http://localhost:8080/cart/product_del/${productId}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    if (response.ok) {
      // Удаляем товар из локального списка после успешного ответа сервера
      cartItems.value = cartItems.value.filter(item => item.id !== productId);
      console.log(`Товар ${productId} удален из корзины`);
    } else {
      throw new Error('Ошибка при удалении товара из корзины');
    }
  } catch (error) {
    console.error(error);
  }
};

// Функция для оформления заказа
const createOrder = async () => {
  if (!userId.value) {
    console.error('Не удалось получить данные о пользователе');
    return;
  }

  if (cartItems.value.length === 0) {
    alert('Корзина пуста. Добавьте товары перед оформлением заказа.');
    return; // Прекращаем выполнение, т.к. нечего заказывать
  }

  const orderData = {
    userId: userId.value,
    products: cartItems.value.map(item => ({
      productId: item.id,
      quantity: item.quantity,
    })),
  };

  try {
    const response = await fetch('http://localhost:8080/order/orders', {
      credentials: 'include',
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(orderData),
    });

    if (!response.ok) {
      throw new Error(`Ошибка при оформлении заказа: ${response.statusText}`);
    }

    // Ждем, пока корзина очистится на сервере и локально
    await clearCart();

    // Показать успешное оформление
    isOrderPlaced.value = true;

    setTimeout(() => {
      isOrderPlaced.value = false;
    }, 2000);
  } catch (error) {
    console.error('Ошибка при оформлении заказа:', error);
  }
};



const clearCart = async () => {
  try {
    for (const item of cartItems.value) {
      const response = await fetch(`http://localhost:8080/cart/product_del/${item.id}`, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
      });

      if (!response.ok) {
        throw new Error(`Ошибка при удалении товара ${item.id}`);
      }
    }
    cartItems.value = []; // Очистить локально после удаления
    console.log('Корзина очищена поэтапно');
  } catch (error) {
    console.error(error);
  }
}

// Увеличение количества товара
const increaseQuantity = (item) => {
  item.quantity++;
};

// Уменьшение количества товара
const decreaseQuantity = (item) => {
  if (item.quantity > 1) {
    item.quantity--;
  }
};

// Подсчет общей суммы заказа
const totalPrice = computed(() => {
  return cartItems.value.reduce((sum, item) => sum + item.price * item.quantity, 0);
});

// Загружаем данные при монтировании компонента
onMounted(() => {
  fetchUser();  // Сначала получаем данные о пользователе
  fetchCartItems();  // Затем загружаем товары в корзину
});
</script>

<template>
  <NavMenu />
  <div class="cart-container">
    <h1>Корзина</h1>

    <div class="cart-grid">
      <!-- Колонка с товарами -->
      <div class="cart-items">
        <div v-for="item in cartItems" :key="item.id" class="cart-item">
          <img :src="item.imagePath" :alt="item.name" class="cart-item-img" />
          <div class="cart-item-info">
            <p class="cart-item-name">{{ item.name }}</p>
            <p class="cart-item-price">{{ item.price.toLocaleString() }} ₽</p>
            <div class="cart-item-controls">
              <button @click="decreaseQuantity(item)">-</button>
              <span>{{ item.quantity }}</span>
              <button @click="increaseQuantity(item)">+</button>
            </div>
          </div>
          <button class="cart-remove" @click="removeItem(item.id)">✖</button>
        </div>
      </div>

      <!-- Колонка с итоговой суммой -->
      <div class="cart-summary">
        <h2>Итого</h2>
        <p class="cart-total">Сумма: <strong>{{ totalPrice.toLocaleString() }} ₽</strong></p>
<button
  class="cart-checkout flex items-center gap-2 px-4 py-2 rounded-md text-white font-medium transition-all duration-500"
  :class="isOrderPlaced ? 'bg-green-500 hover:bg-green-600' : 'bg-blue-600 hover:bg-blue-700'"
  @click="createOrder"
  :disabled="isOrderPlaced"
>
  <component :is="isOrderPlaced ? CheckIcon : ShoppingCartIcon" class="w-5 h-5 transition-transform duration-300" />
  <span class="transition-all duration-300">
    {{ isOrderPlaced ? 'Заказ оформлен' : 'Оформить заказ' }}
  </span>
</button>
      </div>
    </div>
  </div>
</template>

<style scoped></style>
