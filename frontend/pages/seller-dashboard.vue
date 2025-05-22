<template>
      <NavMenu />
  <div class="p-4 dash">
    <Tabs default-value="crm" class="w-full">
      <TabsList>
        <TabsTrigger value="crm">CRM</TabsTrigger>
        <TabsTrigger value="create">Создать товар</TabsTrigger>
        <TabsTrigger value="manage">Управление товарами</TabsTrigger>
      </TabsList>

      <!-- Вкладка CRM -->
      <TabsContent value="crm">
        <Card>
          <CardContent class="p-6">
            <h2 class="text-xl font-semibold mb-4">Добро пожаловать в CRM</h2>
            <p class="text-muted-foreground">
              Здесь вы будете видеть заказы, статистику и управление товарами.
            </p>
          </CardContent>
        </Card>
      </TabsContent>

      <!-- Вкладка создания товара -->
      <TabsContent value="create">
        <Card>
          <CardContent class="p-6">
            <h2 class="text-xl font-semibold mb-4">Добавить новый товар</h2>
            <form @submit.prevent="submitProduct">
              <div class="grid gap-4">
                <div>
                  <label class="block text-sm font-medium">Название</label>
                  <Input v-model="form.Name" placeholder="Smartphone X200" />
                </div>

                <div>
                  <label class="block text-sm font-medium">Описание</label>
                  <Textarea v-model="form.Description" placeholder="Описание товара" />
                </div>

                <div>
                  <label class="block text-sm font-medium">Характеристики</label>
                  <Textarea v-model="form.Characteristic" placeholder="RAM: 12GB, Storage: 256GB..." />
                </div>

                <div>
                  <label class="block text-sm font-medium">Цена (₽)</label>
                  <Input type="number" v-model.number="form.Price" />
                </div>

                <div>
                  <label class="block text-sm font-medium">Количество</label>
                  <Input type="number" v-model.number="form.CountProduct" />
                </div>

                <div>
                  <label class="block text-sm font-medium">Категория (ID)</label>
                  <Input type="number" v-model.number="form.Category" />
                </div>

                <!-- Загрузка изображения -->
                <div>
                  <label class="block text-sm font-medium mb-1">Загрузить изображение</label>
                  <input type="file" accept="image/*" @change="uploadImage" />
                  <div v-if="form.imagePath" class="mt-2">
                    <img
                      :src="form.imagePath"
                      alt="Превью"
                      class="w-32 h-32 object-cover rounded border"
                    />
                  </div>
                </div>

                <div class="text-right mt-4">
                  <Button type="submit">Создать товар</Button>
                </div>
              </div>
            </form>
          </CardContent>
        </Card>
      </TabsContent>

      <!-- Вкладка управления товарами -->
      <TabsContent value="manage">
        <Card>
          <CardContent class="p-6">
            <h2 class="text-xl font-semibold mb-4">Список товаров</h2>
            <div v-if="loading">Загрузка...</div>
            <div v-else>
              <div v-if="products.length === 0">Нет товаров</div>
              <div v-else class="grid gap-4">
                <div v-for="product in products" :key="product.id" class="border p-4 rounded">
                  <h3 class="font-semibold">{{ product.name }}</h3>
                  <p class="text-sm text-muted-foreground">{{ product.description }}</p>
                  <div class="text-sm">Цена: {{ product.price }} ₽</div>

                  <div class="mt-2 flex gap-2">
                    <Button @click="startEdit(product)">Редактировать</Button>
                    <Button variant="destructive" @click="deleteProduct(product.id)">Удалить</Button>
                  </div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </TabsContent>
    </Tabs>

    <!-- Модальное окно редактирования -->
    <div v-if="editingProduct" class="modal-overlay" @click.self="cancelEdit">
      <div class="modal">
        <h2 class="text-xl font-semibold mb-4">Редактирование товара</h2>
        <form @submit.prevent="updateProduct">
          <div class="grid gap-4">
            <div>
              <label class="block text-sm font-medium">Название</label>
              <Input v-model="editForm.name" />
            </div>
            <div>
              <label class="block text-sm font-medium">Описание</label>
              <Textarea v-model="editForm.description" />
            </div>
            <div>
              <label class="block text-sm font-medium">Цена</label>
              <Input type="number" v-model.number="editForm.price" />
            </div>
            <div class="text-right mt-4 flex gap-2">
              <Button type="submit">Сохранить изменения</Button>
              <Button type="button" variant="secondary" @click="cancelEdit">Отмена</Button>
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import '@/assets/css/main.css'
import NavMenu from '@/components/NavMenu/NavMenu.vue';
import { ref, reactive, onMounted } from 'vue'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'

interface Product {
  id: number
  name: string
  description: string
  price: number
  characteristic?: string
  countProduct?: number
  category?: number
  imagePath?: string
}

const form = ref({
  Name: '',
  Description: '',
  Characteristic: '',
  Price: 0,
  CountProduct: 1,
  Category: 1,
  imagePath: '',
})

const products = ref<Product[]>([])
const loading = ref(false)

const editingProduct = ref<Product | null>(null)

const editForm = reactive({
  name: '',
  description: '',
  price: 0,
})

// Загрузка изображения для нового товара
async function uploadImage(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    const response = await fetch('/api/upload', {
      method: 'POST',
      body: formData,
    })

    if (!response.ok) throw new Error('Ошибка загрузки изображения')

    const result = await response.json()
    form.value.imagePath = result.imagePath
  } catch (error) {
    console.error('Ошибка при загрузке изображения:', error)
    alert('❌ Не удалось загрузить изображение')
  }
}

// Создать новый товар
async function submitProduct() {
  if (!form.value.Name || !form.value.Price || !form.value.imagePath) {
    alert('Пожалуйста, заполните все обязательные поля')
    return
  }

  try {
    const response = await fetch('http://localhost:8080/product/create', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: form.value.Name,
        description: form.value.Description,
        characteristic: form.value.Characteristic,
        price: form.value.Price,
        countProduct: form.value.CountProduct,
        category: form.value.Category,
        imagePath: form.value.imagePath,
      }),
      credentials: 'include',
    })

    if (!response.ok) throw new Error('Ошибка при создании товара')

    alert('✅ Товар успешно создан!')
    resetForm()
    await loadProducts()
  } catch (error) {
    console.error(error)
    alert('❌ Ошибка при создании товара')
  }
}

function resetForm() {
  form.value = {
    Name: '',
    Description: '',
    Characteristic: '',
    Price: 0,
    CountProduct: 1,
    Category: 1,
    imagePath: '',
  }
}

// Загрузить список товаров
async function loadProducts() {
  loading.value = true
  try {
    const response = await fetch('http://localhost:8080/product/top_product', {
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Ошибка загрузки товаров')

    const data = await response.json()
    products.value = data
  } catch (error) {
    console.error(error)
    alert('❌ Ошибка при загрузке товаров')
  } finally {
    loading.value = false
  }
}

// Начать редактирование товара (открыть попап)
function startEdit(product: Product) {
  editingProduct.value = product
  editForm.name = product.name
  editForm.description = product.description
  editForm.price = product.price
}

// Отмена редактирования (закрыть попап)
function cancelEdit() {
  editingProduct.value = null
}

// Обновить товар (отправить PUT запрос)
async function updateProduct() {
  if (!editingProduct.value) return

  try {
    const response = await fetch(`http://localhost:8080/product/update/${editingProduct.value.id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: editForm.name,
        description: editForm.description,
        price: editForm.price,
      }),
      credentials: 'include',
    })

    if (!response.ok) throw new Error('Ошибка при обновлении товара')

    alert('✅ Товар успешно обновлен!')
    cancelEdit()
    await loadProducts()
  } catch (error) {
    console.error(error)
    alert('❌ Ошибка при обновлении товара')
  }
}

// Удалить товар с подтверждением
async function deleteProduct(id: number) {
  if (!confirm('Вы уверены, что хотите удалить этот товар?')) return

  try {
    const response = await fetch(`http://localhost:8080/product/delete/${id}`, {
      method: 'DELETE',
      credentials: 'include',
    })

    if (!response.ok) throw new Error('Ошибка при удалении товара')

    alert('✅ Товар успешно удален!')
    await loadProducts()
  } catch (error) {
    console.error(error)
    alert('❌ Ошибка при удалении товара')
  }
}

// При загрузке компонента получаем товары
onMounted(() => {
  loadProducts()
})
</script>

<style scoped>
/* Простое оформление модального окна */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal {
  background: white;
  padding: 1.5rem;
  border-radius: 0.5rem;
  width: 400px;
  max-width: 90%;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
}

.dash{
        margin-bottom: 50px;
}
</style>
